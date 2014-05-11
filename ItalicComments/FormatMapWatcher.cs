using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;

namespace CommentsPlus.ItalicComments
{
    internal sealed class FormatMapWatcher : IDisposable
    {
        bool _inUpdate = false;
        IClassificationFormatMap _formatMap;
        IClassificationTypeRegistryService _typeRegistry;
        ITextView _view;

        static readonly List<string> CommentTypes = new List<string>() { "comment", "xml doc comment", "vb xml doc comment", "xml comment", "html comment", "xaml comment" };
        static readonly List<string> DocTagTypes = new List<string>() { "xml doc tag", "vb xml doc tag", "xml doc attribute" };

        public FormatMapWatcher(ITextView view, IClassificationFormatMap formatMap, IClassificationTypeRegistryService typeRegistry)
        {
            _view = view;
            _formatMap = formatMap;
            _typeRegistry = typeRegistry;

            FixComments();

            //_formatMap.ClassificationFormatMappingChanged += FormatMapChanged;

            view.GotAggregateFocus += FirstGotFocus;
            view.Closed += view_Closed;
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.Closed -= view_Closed;
                _view.GotAggregateFocus -= FirstGotFocus;
                _view = null;
            }

            if (_formatMap != null)
            {
                _formatMap.ClassificationFormatMappingChanged -= FormatMapChanged;
                _formatMap = null;
            }
            _typeRegistry = null;
        }

        void view_Closed(object sender, EventArgs e)
        {
            var view = sender as ITextView;
            if (view != null)
                view.Closed -= view_Closed;

            Dispose();
        }

        void FirstGotFocus(object sender, EventArgs e)
        {
            var view = sender as ITextView;
            if (view != null)
                view.GotAggregateFocus -= FirstGotFocus;

            Debug.Assert(!_inUpdate, "How can we be updating *while* the view is getting focus?");

            FixComments();
        }

        void FormatMapChanged(object sender, EventArgs e)
        {
            FixComments();
        }

        internal void FixComments()
        {
            if (_inUpdate || _formatMap == null || (_view != null && _view.IsClosed))
                return;

            bool batch = false;
            try
            {
                _inUpdate = true;

                if (!_formatMap.IsInBatchUpdate)
                {
                    _formatMap.BeginBatchUpdate();
                    batch = true;
                }

                // First, go through the ones we know about:

                // 1) Known comment types are italicized
                foreach (var type in CommentTypes.Select(t => _typeRegistry.GetClassificationType(t)).Where(t => t != null))
                {
                    Italicize(type);
                }

                // 2) Known doc tags
                foreach (var type in DocTagTypes.Select(t => _typeRegistry.GetClassificationType(t)).Where(t => t != null))
                {
                    Italicize(type);
                }

                // 3) Grab everything else that looks like a comment or doc tag
                foreach (var classification in _formatMap.CurrentPriorityOrder.Where(c => c != null))
                {
                    string name = classification.Classification;
                    var comparer = StringComparer.OrdinalIgnoreCase;
                    if (CommentTypes.Contains(name, comparer) || DocTagTypes.Contains(name, comparer))
                        continue;

                    if (name.IndexOf("comment", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("doc tag", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Italicize(classification);
                    }
                }
            }
            finally
            {
                if (batch && _formatMap.IsInBatchUpdate)
                    _formatMap.EndBatchUpdate();
                _inUpdate = false;
            }
        }

        /// <summary>Set italics for classification.</summary>
        /// <param name="classification">The classification to process.</param>
        void Italicize(IClassificationType classification)
        {
            /* Get the classification text properties */
            var properties = _formatMap.GetTextProperties(classification);

            //If italics has already been determined, skip it
            if (!properties.ItalicEmpty)
                return;

            //add italics 
            properties = properties.SetItalic(true);

            // And put it back in the format map
            _formatMap.SetTextProperties(classification, properties);
        }
    }
}
