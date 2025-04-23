//#define DIAG_TIMING
//#define YIELD_TAGS
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
        internal IBufferTagAggregatorFactoryService TagAggregatorFactory = null;

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            return new CommentTagger(ClassificationRegistry, TagAggregatorFactory, buffer) as ITagger<T>;
        }
    }

    class CommentTagger : ITagger<ClassificationTag>
    {
        readonly Dictionary<Classification, ClassificationTag> _classifications;
        readonly Dictionary<Classification, ClassificationTag> _htmlClassifications;
        readonly Dictionary<Classification, ClassificationTag> _xmlClassifications;

        readonly IAccurateTagAggregator<IClassificationTag> _aggregator;

        static bool _enabled;

        static readonly string[] Comments = { "//", "'", "#", "<!--"/*, "/*"*/ };

        static readonly string[] ImportantComments = { "! ", "# " }; // #! Shebang not really useful in Python and the like since it already has a meaning!?
        static readonly string[] QuestionComments = { "? " };
        static readonly string[] WtfComments = { "!? ", "‽ ", "WTF ", "WTF: "/*, "WAT ", "WAT: "*/ }; //ಠ_ಠ
        static readonly string[] RemovedComments = { "x ", "¤ ", "// ", "//" };
        static readonly string[] TaskComments = { "TODO ", "TODO:", "TODO@", "HACK ", "HACK:" };
        static readonly string[] RainbowComments = { "+? " }; //シ  

#if !YIELD_TAGS
        static readonly List<ITagSpan<ClassificationTag>> EmptyTags = new List<ITagSpan<ClassificationTag>>();
#endif

#pragma warning disable 67
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
#pragma warning restore 67

        internal CommentTagger(IClassificationTypeRegistryService registry, IBufferTagAggregatorFactoryService factory, ITextBuffer buffer)
        {
            _classifications = new string[] { Constants.ImportantComment, Constants.QuestionComment, Constants.WtfComment, Constants.RemovedComment, Constants.TaskComment, Constants.RainbowComment }
                .ToDictionary(GetClassification, s => new ClassificationTag(registry.GetClassificationType(s)));

            _htmlClassifications = new string[] { Constants.ImportantHtmlComment, Constants.QuestionHtmlComment, Constants.WtfComment, Constants.RemovedHtmlComment, Constants.TaskHtmlComment }
                .ToDictionary(GetClassification, s => new ClassificationTag(registry.GetClassificationType(s)));

            _xmlClassifications = new string[] { Constants.ImportantXmlComment, Constants.QuestionXmlComment, Constants.WtfComment, Constants.RemovedXmlComment, Constants.TaskXmlComment }
                .ToDictionary(GetClassification, s => new ClassificationTag(registry.GetClassificationType(s)));

            var aggregator = factory.CreateTagAggregator<IClassificationTag>(buffer);
            _aggregator = aggregator as IAccurateTagAggregator<IClassificationTag>;
        }

        static CommentTagger()
        {
            _enabled = IsEnabled();
        }

        static bool IsEnabled()
        {
            bool res = RegistryHelper.IsEnabled("EnableTags", true);
            return res;
        }

        /// <summary>
        /// Gets all the tags that intersect the specified spans.
        /// </summary>
        /// <param name="spans">The spans to visit.</param>
        /// <returns>A TagSpan for each tag.</returns>
        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
#if DIAG_TIMING
            var sw = Stopwatch.StartNew();

            //ToList seems to perform better than returning the iterator
            var tags = GetTagsInternal(spans).AsList();

            sw.Stop();
            if (sw.Elapsed > TimeSpan.FromMilliseconds(1))
                Trace.WriteLine("GetTags took: " + sw.Elapsed, "CommentsPlus");

            return tags;
#else
            return GetTagsInternal(spans);
#endif
        }

        private IEnumerable<ITagSpan<ClassificationTag>> GetTagsInternal(NormalizedSnapshotSpanCollection spans, CancellationToken? cancellation = default)
        {
            if (!_enabled || spans.Count == 0)
#if YIELD_TAGS
                yield break;
#else
                return EmptyTags;
#endif

            ITextSnapshot snapshot = spans[0].Snapshot;
            var contentType = snapshot.TextBuffer.ContentType;
            if (!contentType.IsOfType("code"))
#if YIELD_TAGS
                yield break;
#else
                return EmptyTags;
#endif

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

#if !YIELD_TAGS
            List<ITagSpan<ClassificationTag>> resultTags = null;
#endif
            string previousCommentType = null;

            bool all = contentType.IsOfType("CSharp") || contentType.IsOfType("Basic");
            var currentTagSpans = all ? _aggregator.GetAllTags(spans, cancellation ?? default) : _aggregator.GetTags(spans);
            foreach (var tagSpan in currentTagSpans)
            {
                if (cancellation.HasValue && cancellation.Value.IsCancellationRequested)
#if YIELD_TAGS
                    yield break;
#else
                    break;
#endif
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

#if YIELD_TAGS
                        yield return outTagSpan;
#else
                        if (resultTags == null)
                            resultTags = new List<ITagSpan<ClassificationTag>>();

                        resultTags.Add(outTagSpan);
#endif
                    }
                }
            }

#if !YIELD_TAGS
            return resultTags ?? EmptyTags;
#endif
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
