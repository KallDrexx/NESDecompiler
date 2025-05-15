using System.Windows;

namespace NESDecompiler.GUI
{
    public partial class InputDialog : Window
    {
        public string ResponseText
        {
            get { return ResponseTextBox.Text; }
            set { ResponseTextBox.Text = value; }
        }

        public InputDialog(string title, string prompt, string defaultResponse = "")
        {
            InitializeComponent();

            Title = title;
            PromptText.Text = prompt;
            ResponseText = defaultResponse;

            ResponseTextBox.SelectAll();
            ResponseTextBox.Focus();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}