using System;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using ICSharpCode.AvalonEdit;

namespace NESDecompiler.GUI
{
    /// <summary>
    /// Behavior for binding the text of an AvalonEdit control
    /// </summary>
    public class AvalonEditBehavior : Behavior<TextEditor>
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(AvalonEditBehavior),
                new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback));

        /// <summary>
        /// The bound text property
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Called when the Text property changes
        /// </summary>
        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as AvalonEditBehavior;
            if (behavior != null && behavior.AssociatedObject != null)
            {
                var editor = behavior.AssociatedObject;

                if (editor.Text != e.NewValue as string)
                {
                    editor.Text = e.NewValue as string ?? string.Empty;
                }
            }
        }

        /// <summary>
        /// Called when the behavior is attached to the control
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject != null)
            {
                AssociatedObject.Text = Text ?? string.Empty;

                AssociatedObject.TextChanged += OnTextChanged;
            }
        }

        /// <summary>
        /// Called when the behavior is detached from the control
        /// </summary>
        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged -= OnTextChanged;
            }

            base.OnDetaching();
        }

        /// <summary>
        /// Called when the text in the editor changes
        /// </summary>
        private void OnTextChanged(object? sender, EventArgs e)
        {
            if (AssociatedObject != null)
            {
                Text = AssociatedObject.Text;
            }
        }
    }
}