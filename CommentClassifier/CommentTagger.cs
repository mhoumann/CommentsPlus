//#define DIAG_TIMING
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace CommentsPlus.CommentClassifier
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("code")]
    [TagType(typeof(ClassificationTag))]
    [Name("CommentsPlusTagger")]
    public class CommentTaggerProvider : IViewTaggerProvider
    {
        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry = null;

        [Import]
        internal IBufferTagAggregatorFactoryService BufferTagAggregatorFactory = null;

        [Import]
        internal IViewTagAggregatorFactoryService ViewTagAggregatorFactory = null;

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            return textView.Properties.GetOrCreateSingletonProperty(() => new CommentTagger(ClassificationRegistry, ViewTagAggregatorFactory, BufferTagAggregatorFactory, textView, buffer) as ITagger<T>);
        }
    }

    sealed class CommentTagger : ITagger<ClassificationTag>
    {
        readonly Dictionary<Classification, ClassificationTag> _classifications;
        readonly Dictionary<Classification, ClassificationTag> _htmlClassifications;
        readonly Dictionary<Classification, ClassificationTag> _xmlClassifications;

        readonly ITextView _textView;
        readonly ITextBuffer _buffer;
        readonly IViewTagAggregatorFactoryService _viewTagAggregatorFactory;
        readonly IBufferTagAggregatorFactoryService _bufferTagAggregatorFactory;

        readonly List<ITagSpan<ClassificationTag>> _resultTags = new List<ITagSpan<ClassificationTag>>();

        readonly long _instance = ++instanceCounter;

        ITagAggregator<IClassificationTag> _aggregator;
        bool _working = false;

        static long instanceCounter = 0;

        static readonly bool IsEnabled;
        static readonly bool UseBufferTagAggregator = false;

        static readonly string[] Comments = { "//", "'", "#", "<!--"/*, "/*"*/ };

        static readonly string[] ImportantComments = { "! ", "# " }; // #! Shebang not really useful in Python and the like since it already has a meaning!?
        static readonly string[] QuestionComments = { "? " };
        static readonly string[] WtfComments = { "!? ", "‽ ", "WTF ", "WTF: "/*, "WAT ", "WAT: "*/ }; //ಠ_ಠ
        static readonly string[] RemovedComments = { "x ", "¤ ", "// ", "//" };
        static readonly string[] TaskComments = { "TODO ", "TODO:", "TODO@", "HACK ", "HACK:" };
        static readonly string[] RainbowComments = { "+? " }; //シ  

#if DIAG_TIMING
        long TimingCount = 0;
#endif

#pragma warning disable 67
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
#pragma warning restore 67

        internal CommentTagger(IClassificationTypeRegistryService registry, IViewTagAggregatorFactoryService viewAggrFactory, IBufferTagAggregatorFactoryService factory, ITextView textView, ITextBuffer buffer)
        {
            _classifications = new string[] { Constants.ImportantComment, Constants.QuestionComment, Constants.WtfComment, Constants.RemovedComment, Constants.TaskComment, Constants.RainbowComment }
                .ToDictionary(GetClassification, s => new ClassificationTag(registry.GetClassificationType(s)));

            _htmlClassifications = new string[] { Constants.ImportantHtmlComment, Constants.QuestionHtmlComment, Constants.WtfComment, Constants.RemovedHtmlComment, Constants.TaskHtmlComment }
                .ToDictionary(GetClassification, s => new ClassificationTag(registry.GetClassificationType(s)));

            _xmlClassifications = new string[] { Constants.ImportantXmlComment, Constants.QuestionXmlComment, Constants.WtfComment, Constants.RemovedXmlComment, Constants.TaskXmlComment }
                .ToDictionary(GetClassification, s => new ClassificationTag(registry.GetClassificationType(s)));

            _textView = textView;
            _buffer = buffer;
            _viewTagAggregatorFactory = viewAggrFactory;
            _bufferTagAggregatorFactory = factory;
        }

        static CommentTagger()
        {
            IsEnabled = RegistryHelper.IsEnabled("EnableTags", true);
            UseBufferTagAggregator = RegistryHelper.IsEnabled("UseOldTagging", false);
        }

        /// <summary>
        /// Gets all the tags that intersect the specified spans.
        /// </summary>
        /// <param name="spans">The spans to visit.</param>
        /// <returns>A <see cref="Microsoft.VisualStudio.Text.Tagging.ITagSpan{T}"/> for each tag.</returns>
        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (_aggregator == null)
            {
                var contentType = _buffer.ContentType;
                bool isRoslyn = contentType.IsOfType("CSharp") || contentType.IsOfType("Basic");
                bool useViewTagAggregator = isRoslyn || !UseBufferTagAggregator;

                if (_viewTagAggregatorFactory != null && useViewTagAggregator)
                    _aggregator = _viewTagAggregatorFactory.CreateTagAggregator<IClassificationTag>(_textView);
                else
                    _aggregator = _bufferTagAggregatorFactory.CreateTagAggregator<IClassificationTag>(_buffer);
            }

            if (_working)
                return Enumerable.Empty<ITagSpan<ClassificationTag>>();

            _working = true;
            try
            {
                return GetTagsInternal(spans);
            }
            finally
            {
                _working = false;
            }
        }

        private IEnumerable<ITagSpan<ClassificationTag>> GetTagsInternal(NormalizedSnapshotSpanCollection spans, CancellationToken? cancellation = default)
        {
            if (!IsEnabled || spans.Count == 0)
                return Enumerable.Empty<ITagSpan<ClassificationTag>>();

#if DIAG_TIMING
            var sw = Stopwatch.StartNew();
#endif
            ITextSnapshot snapshot = spans[0].Snapshot;
            var contentType = snapshot.TextBuffer.ContentType;
            if (!contentType.IsOfType("code"))
                return Enumerable.Empty<ITagSpan<ClassificationTag>>();

            bool isMarkup = false;

            var lookup = _classifications;
            if (IsHtmlMarkup(contentType))
            {
                lookup = _htmlClassifications;
                isMarkup = true;
            }
            else if (IsXmlMarkup(contentType))
            {
                lookup = _xmlClassifications;
                isMarkup = true;
            }

            _resultTags.Clear();
            string previousCommentType = null;

#if USE_ACCURATE_TAGGER
            bool isRoslyn = contentType.IsOfType("CSharp") || contentType.IsOfType("Basic");
            bool all = isRoslyn;
            // Use IAccurateTagAggregator<T>.GetAllTags for C# and VB only - seems to have serious perf issues with some other languages (.py, .js)
            // NOTE: Perf is not very good for editor even for C#/VB
            var currentTagSpans = (all && _aggregator is IAccurateTagAggregator<IClassificationTag> accAggregator)
                ? accAggregator.GetAllTags(spans, cancellation ?? default)
                : _aggregator.GetTags(spans);
#else
            var currentTagSpans = _aggregator.GetTags(spans);
#endif
            foreach (var tagSpan in currentTagSpans)
            {
                if (cancellation.HasValue && cancellation.Value.IsCancellationRequested)
                    break;

                // find spans that the language service has already classified as comments ...
                string classificationName = tagSpan.Tag.ClassificationType.Classification;
                if (!classificationName.Contains("comment", StringComparison.OrdinalIgnoreCase))
                    continue;

                var nssc = tagSpan.Span.GetSpans(snapshot);
                if (nssc.Count > 0)
                {
                    // ... and from those, ones that match our comment strings
                    var snapshotSpan = nssc[0];

                    string text = snapshotSpan.GetText();
                    if (string.IsNullOrWhiteSpace(text))
                        continue;

                    string startCommentOnlyType = text.EqualsOneOf(Comments);
                    if (startCommentOnlyType != null)
                    {
                        previousCommentType = startCommentOnlyType;
                        continue;
                    }

                    //NOTE: markup comment span does not include comment start token
                    //NOTE: .js/.ts comment span has changed to not include comment start token
                    string commentType = text.StartsWithOneOf(Comments);
                    if (commentType == null)
                    {
                        if (!isMarkup && previousCommentType == null)
                            continue;

                        commentType = "";
                    }

                    previousCommentType = null;

                    int startIndex = commentType.Length;

                    int endTokenLength = 0;
                    if (isMarkup && commentType.Length > 0)
                    {
                        if (!text.EndsWith("-->"))
                            continue;

                        endTokenLength = 3;
                    }

                    //¤ int? removedSpanLength = null;
                    ClassificationTag ctag = null;
                    string match;
                    if (Match(text, startIndex, ImportantComments, out match))
                    {
                        ctag = lookup[Classification.Important];
                    }
                    else if (Match(text, startIndex, QuestionComments, out match))
                    {
                        ctag = lookup[Classification.Question];
                    }
                    else if (Match(text, startIndex, RemovedComments, out match))
                    {
                        if (commentType == "//" && match == "//")
                        {
                            int len = startIndex + match.Length;
                            if (text.Length > len && text[len] != '/')
                                ctag = lookup[Classification.Removed];
                        }
                        else
                            ctag = lookup[Classification.Removed];

                        //¤ int index = text.IndexOf(commentType, startIndex + match.Length, StringComparison.Ordinal);
                        //¤ if (index > 0)
                        //¤ {
                        //¤     int len = text.Length - index;
                        //¤     removedSpanLength = text.Length - (startIndex + match.Length + len);
                        //¤ }
                    }
                    else if (Match(text, startIndex, TaskComments, StringComparison.OrdinalIgnoreCase, true, out match))
                    {
                        bool fix = FixTaskComment(text, startIndex, ref match);
                        ctag = lookup[Classification.Task];
                    }
                    else if (Match(text, startIndex, WtfComments, out match))
                    {
                        ctag = lookup[Classification.Wtf];
                    }
                    else if (Match(text, startIndex, RainbowComments, out match))
                    {
                        ctag = lookup[Classification.Rainbow];
                    }

                    if (ctag != null)
                    {
                        int matchLength = commentType.Length + match.Length;

                        int spanLength = /*removedSpanLength ??*/ (snapshotSpan.Length - (matchLength + endTokenLength));

                        var span = new SnapshotSpan(snapshotSpan.Snapshot, snapshotSpan.Start + matchLength, spanLength);
                        var outTagSpan = new TagSpan<ClassificationTag>(span, ctag);

                        _resultTags.Add(outTagSpan);
                    }
                }
            }

#if DIAG_TIMING
            sw.Stop();
            if (sw.Elapsed > TimeSpan.FromMilliseconds(10) || (TimingCount++ == 1 || TimingCount % 10 == 0))
                Trace.WriteLine($"GetTags ({_instance}) time ({TimingCount}): {sw.Elapsed}", "CommentsPlus");
#endif
            return _resultTags;
        }

        private static bool FixTaskComment(string text, int startIndex, ref string match)
        {
            bool res = false;
            if (match != null && match.EndsWith("@", StringComparison.Ordinal))
            {
                int index = text.IndexOfAny(new char[] { ' ', '\t', ':' }, startIndex + match.Length);
                if (index >= 0)
                {
                    int len = (index - startIndex) + 1;
                    match = text.Substring(startIndex, len);
                    res = true;
                }
            }
            return res;
        }

        static bool Match(string commentText, int startIndex, string[] templates, out string match)
        {
            return Match(commentText, startIndex, templates, StringComparison.Ordinal, false, out match);
        }

        static bool Match(string commentText, int startIndex, string[] templates, StringComparison comparison, bool allowLeadingWhiteSpace, out string match)
        {
            bool lws = false;
            if (allowLeadingWhiteSpace && commentText.StartsWithWhiteSpace(startIndex))
            {
                lws = true;
                startIndex += 1;
            }
            match = commentText.StartsWithOneOf(startIndex, templates, comparison);
            if (lws && match != null)
                match = commentText.Substring(startIndex - 1, match.Length + 1);
            return match != null;
        }

        static Classification GetClassification(string s)
        {
            if (s.Contains("Important"))
                return Classification.Important;
            if (s.Contains("Question"))
                return Classification.Question;
            if (s.Contains("Removed"))
                return Classification.Removed;
            if (s.Contains("Task"))
                return Classification.Task;
            if (s.Contains("WAT"))
                return Classification.Wtf;
            if (s.Contains("Rainbow"))
                return Classification.Rainbow;

            throw new ArgumentException("Unknown classification type");
        }

        private static bool IsHtmlMarkup(IContentType contentType)
        {
            bool res = contentType.IsOfType("html") || contentType.IsOfType("htmlx");
            return res;
        }

        private static bool IsXmlMarkup(IContentType contentType)
        {
            bool res = contentType.IsOfType("XAML") || contentType.IsOfType("XML");
            return res;
        }
    }
}
