using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AzureTools.Services;

namespace AzureTools.Views
{
    public partial class PromptDialog : Window
    {
        public string Prompt { get; }
        public string DialogTitle { get; }
        public string Input { get; set; }

        public PromptDialog(string title, string prompt, PromptDialogOption dialogOption)
        {
            InitializeComponent();
            DialogTitle = title;
            Prompt = prompt;
            Input = dialogOption.DefaultValue ?? string.Empty;
            DataContext = this;
            if (!dialogOption.IsPassword)
            {
                InputTextBox.PasswordChar = '\0';
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            Close(Input);
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close(null);
        }
    }
}
