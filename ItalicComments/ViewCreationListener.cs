using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Win32;

namespace CommentsPlus.ItalicComments
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class ViewCreationListener : IWpfTextViewCreationListener
    {
        [Import]
        IClassificationFormatMapService formatMapService = null;

        [Import]
        IClassificationTypeRegistryService typeRegistry = null;

        static bool _isEnabled;

        static ViewCreationListener()
        {
            _isEnabled = IsEnabled();
        }

        /// <summary>
        /// When a text view is created, make all comments italicized.
        /// </summary>
        /// <param name="textView">The view to handle.</param>
        public void TextViewCreated(IWpfTextView textView)
        {
            if (_isEnabled)
                textView.Properties.GetOrCreateSingletonProperty(() => new FormatMapWatcher(textView, formatMapService.GetClassificationFormatMap(textView), typeRegistry));
        }

        static bool IsEnabled()
        {
            bool res = RegistryHelper.IsEnabled("EnableItalics", false);
            return res;
        }
    }
}