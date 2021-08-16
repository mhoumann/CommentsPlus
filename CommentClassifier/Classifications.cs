using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;

using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace CommentsPlus.CommentClassifier
{
    //The quick brown fox jumps over the lazy dog
    //! Important note
    //? What's all this?
    //TODO Work remaining
    //ToDo@NN: Some work remaining for you
    //x object q = dt(42); //What is your question?
    //// double pi = Math.PI;
    //!? Wait, what‽

    //TODO: More work remaining
    // TODO: Even more work remaining

    //# Bold Test
    //¤ Removed ¤
    //x Removed x
    //WTF I don't even?
    //‽ Not in the slightest
    //HACK Hackety hack ack!
    //HACK: Not a crack
    // HACK: Slash!
    /////////////////////////////////////////
    ////string commentedOut = OldMethod(a++); /* an old style comment */

    /*? hallo for en kommentar!? */
    /*! A long comment - will it get bold!? 
     * Should this be bold as well?
     * Another line
   */

    //+? The quick brown fox jumps over the lazy dog! 0123456789 AbBbCcDdEeFf Jackdaws love my big sphinx of quartz.

    enum Classification
    {
        None,
        Important,
        Question,
        Wtf,
        Removed,
        Task,
        Rainbow
    }

    /*!? Normal comment - should be italics '*/
    static class Constants
    {
        //! Important
        public const string ImportantComment = "Comment - Important";
        public const string ImportantHtmlComment = "HTML Comment - Important";
        public const string ImportantXmlComment = "XML Comment - Important";
        //? Question
        public const string QuestionComment = "Comment - Question";
        public const string QuestionHtmlComment = "HTML Comment - Question";
        public const string QuestionXmlComment = "XML Comment - Question";
        //!? WAT
        public const string WtfComment = "Comment - WAT!?";

        //x Removed
        public const string RemovedComment = "Comment - Removed";
        public const string RemovedHtmlComment = "HTML Comment - Removed";
        public const string RemovedXmlComment = "XML Comment - Removed";
        //TODO: This does not need work
        public const string TaskComment = "Comment - Task";
        public const string TaskHtmlComment = "HTML Comment - Task";
        public const string TaskXmlComment = "XML Comment - Task";

        public const string RainbowComment = "Comment - Rainbow";

        //x public const string LargeComment = "Comment + Large";
        //x public const string LargerComment = "Comment ++ Large";

        public static readonly Color ImportantColor = Colors.Green;
        public static readonly Color QuestionColor = Colors.Red;
        public static readonly Color WtfColor = Colors.Purple;
        public static readonly Color RemovedColor = Colors.Gray;
        public static readonly Color TaskColor = Color.FromRgb(192, 96, 0);
    }

    public static class ClassificationDefinitions
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [BaseDefinition("Comment")]
        [Name(Constants.ImportantComment)]
        internal static ClassificationTypeDefinition ImportantCommentClassificationType = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [BaseDefinition("Comment")]
        [Name(Constants.QuestionComment)]
        internal static ClassificationTypeDefinition QuestionCommentClassificationType = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [BaseDefinition("Comment")]
        [Name(Constants.WtfComment)]
        internal static ClassificationTypeDefinition WtfCommentClassificationType = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [BaseDefinition("Comment")]
        [Name(Constants.RemovedComment)]
        internal static ClassificationTypeDefinition StrikeoutCommentClassificationType = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [BaseDefinition("Comment")]
        [Name(Constants.TaskComment)]
        internal static ClassificationTypeDefinition TaskCommentClassificationType = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [BaseDefinition("Comment")]
        [Name(Constants.RainbowComment)]
        internal static ClassificationTypeDefinition RainbowCommentClassificationType = null;

        #region HTML

        [Export(typeof(ClassificationTypeDefinition))]
        [BaseDefinition("HTML Comment")]
        [Name(Constants.ImportantHtmlComment)]
        internal static ClassificationTypeDefinition ImportantHtmlCommentClassificationType = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [BaseDefinition("HTML Comment")]
        [Name(Constants.QuestionHtmlComment)]
        internal static ClassificationTypeDefinition QuestionHtmlCommentClassificationType = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [BaseDefinition("HTML Comment")]
        [Name(Constants.RemovedHtmlComment)]
        internal static ClassificationTypeDefinition StrikeoutHtmlCommentClassificationType = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [BaseDefinition("HTML Comment")]
        [Name(Constants.TaskHtmlComment)]
        internal static ClassificationTypeDefinition TaskHtmlCommentClassificationType = null;

        #endregion

        #region XML

        [Export(typeof(ClassificationTypeDefinition))]
        [BaseDefinition("XML Comment")]
        [Name(Constants.ImportantXmlComment)]
        internal static ClassificationTypeDefinition ImportantXmlCommentClassificationType = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [BaseDefinition("XML Comment")]
        [Name(Constants.QuestionXmlComment)]
        internal static ClassificationTypeDefinition QuestionXmlCommentClassificationType = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [BaseDefinition("XML Comment")]
        [Name(Constants.RemovedXmlComment)]
        internal static ClassificationTypeDefinition StrikeoutXmlCommentClassificationType = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [BaseDefinition("Xml Comment")]
        [Name(Constants.TaskXmlComment)]
        internal static ClassificationTypeDefinition TaskXmlCommentClassificationType = null;

        #endregion
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.ImportantComment)]
    [Name(Constants.ImportantComment)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    public sealed class ImportantCommentFormat : ClassificationFormatDefinition
    {
        public ImportantCommentFormat()
        {
            this.DisplayName = Constants.ImportantComment + " (//!)";
            this.ForegroundColor = Constants.ImportantColor;
            this.IsBold = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.QuestionComment)]
    [Name(Constants.QuestionComment)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    public sealed class QuestionCommentFormat : ClassificationFormatDefinition
    {
        public QuestionCommentFormat()
        {
            this.DisplayName = Constants.QuestionComment + " (//?)";
            this.ForegroundColor = Constants.QuestionColor;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.WtfComment)]
    [Name(Constants.WtfComment)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    public sealed class WtfCommentFormat : ClassificationFormatDefinition
    {
        public WtfCommentFormat()
        {
            this.DisplayName = Constants.WtfComment + " (//!?)";
            this.ForegroundColor = Constants.WtfColor;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.RemovedComment)]
    [Name(Constants.RemovedComment)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    public sealed class StrikeoutCommentFormat : ClassificationFormatDefinition
    {
        public StrikeoutCommentFormat()
        {
            this.DisplayName = Constants.RemovedComment + " (//x)";
            this.ForegroundColor = Constants.RemovedColor;
            this.TextDecorations = System.Windows.TextDecorations.Strikethrough;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.TaskComment)]
    [Name(Constants.TaskComment)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    public sealed class TaskCommentFormat : ClassificationFormatDefinition
    {
        public TaskCommentFormat()
        {
            this.DisplayName = Constants.TaskComment + " (//TODO)";
            this.ForegroundColor = Constants.TaskColor;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.RainbowComment)]
    [Name(Constants.RainbowComment)]
    [UserVisible(false)]
    [Order(After = Priority.High)]
    public sealed class RainbowCommentFormat : ClassificationFormatDefinition
    {
        public RainbowCommentFormat()
        {
            this.DisplayName = Constants.RainbowComment + " (//+?)";
            this.ForegroundBrush = new LinearGradientBrush(new GradientStopCollection() {
                new GradientStop(Colors.Red, 0.0),
                new GradientStop(Colors.Orange, 0.17),
                new GradientStop(Colors.Goldenrod, 0.33),
                new GradientStop(Colors.Lime, 0.5),
                new GradientStop(Colors.Blue, 0.66),
                new GradientStop(Colors.Indigo, 0.83),
                new GradientStop(Colors.Violet, 1.0),
            }, 0.0);
            this.IsBold = true;
        }
    }

    #region HTML

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.ImportantHtmlComment)]
    [Name(Constants.ImportantHtmlComment)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    public sealed class ImportantHtmlCommentFormat : ClassificationFormatDefinition
    {
        public ImportantHtmlCommentFormat()
        {
            this.DisplayName = Constants.ImportantHtmlComment + " (<!--!)";
            this.ForegroundColor = Constants.ImportantColor;
            this.IsBold = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.QuestionHtmlComment)]
    [Name(Constants.QuestionHtmlComment)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    public sealed class QuestionHtmlCommentFormat : ClassificationFormatDefinition
    {
        public QuestionHtmlCommentFormat()
        {
            this.DisplayName = Constants.QuestionHtmlComment + " (<!--?)";
            this.ForegroundColor = Constants.QuestionColor;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.RemovedHtmlComment)]
    [Name(Constants.RemovedHtmlComment)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    public sealed class StrikeoutHtmlCommentFormat : ClassificationFormatDefinition
    {
        public StrikeoutHtmlCommentFormat()
        {
            this.DisplayName = Constants.RemovedHtmlComment + " (<!--x)";
            this.ForegroundColor = Constants.RemovedColor;
            this.TextDecorations = System.Windows.TextDecorations.Strikethrough;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.TaskHtmlComment)]
    [Name(Constants.TaskHtmlComment)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    public sealed class TaskHtmlCommentFormat : ClassificationFormatDefinition
    {
        public TaskHtmlCommentFormat()
        {
            this.DisplayName = Constants.TaskHtmlComment + " (<!--TODO)";
            this.ForegroundColor = Constants.TaskColor;
        }
    }

    #endregion

    #region XML

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.ImportantXmlComment)]
    [Name(Constants.ImportantXmlComment)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    public sealed class ImportantXmlCommentFormat : ClassificationFormatDefinition
    {
        public ImportantXmlCommentFormat()
        {
            this.DisplayName = Constants.ImportantXmlComment + " (<!--!)";
            this.ForegroundColor = Constants.ImportantColor;
            this.IsBold = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.QuestionXmlComment)]
    [Name(Constants.QuestionXmlComment)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    public sealed class QuestionXmlCommentFormat : ClassificationFormatDefinition
    {
        public QuestionXmlCommentFormat()
        {
            this.DisplayName = Constants.QuestionXmlComment + " (<!--?)";
            this.ForegroundColor = Constants.QuestionColor;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.RemovedXmlComment)]
    [Name(Constants.RemovedXmlComment)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    public sealed class StrikeoutXmlCommentFormat : ClassificationFormatDefinition
    {
        public StrikeoutXmlCommentFormat()
        {
            this.DisplayName = Constants.RemovedXmlComment + " (<!--x)";
            this.ForegroundColor = Constants.RemovedColor;
            this.TextDecorations = System.Windows.TextDecorations.Strikethrough;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.TaskXmlComment)]
    [Name(Constants.TaskXmlComment)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    public sealed class TaskXmlCommentFormat : ClassificationFormatDefinition
    {
        public TaskXmlCommentFormat()
        {
            this.DisplayName = Constants.TaskXmlComment + " (<!--TODO)";
            this.ForegroundColor = Constants.TaskColor;
        }
    }

    #endregion
}
