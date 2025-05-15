using System;
using System.Windows;
using ICSharpCode.AvalonEdit;

namespace NESDecompiler.GUI
{
    /// <summary>
    /// Provides attached properties for binding text to AvalonEdit
    /// </summary>
    public static class AvalonEditTextBindingBehavior
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached(
                "Text",
                typeof(string),
                typeof(AvalonEditTextBindingBehavior),
                new FrameworkPropertyMetadata(
                    default(string),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnTextChanged));

        /// <summary>
        /// Gets the text value
        /// </summary>
        public static string GetText(DependencyObject obj)
        {
            return (string)obj.GetValue(TextProperty);
        }

        /// <summary>
        /// Sets the text value
        /// </summary>
        public static void SetText(DependencyObject obj, string value)
        {
            obj.SetValue(TextProperty, value);
        }

        /// <summary>
        /// Called when the text property changes
        /// </summary>
        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextEditor editor)
            {
                if (editor.Text != (string)e.NewValue)
                {
                    editor.Text = (string)e.NewValue ?? string.Empty;
                }

                if (e.OldValue != null)
                {
                    editor.TextChanged -= Editor_TextChanged;
                }

                if (e.NewValue != null)
                {
                    editor.TextChanged += Editor_TextChanged;
                }
            }
        }

        /// <summary>
        /// Called when the editor text changes
        /// </summary>
        private static void Editor_TextChanged(object sender, EventArgs e)
        {
            if (sender is TextEditor editor)
            {
                SetText(editor, editor.Text);
            }
        }
    }
}