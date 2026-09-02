using Avalonia.Controls;
using Avalonia.Interactivity;
using AzureTools.Services;
using System;

namespace AzureTools.Views
{
    public partial class PromptDialog : Window
    {

        public PromptDialog(string title, string prompt, PromptDialogOption dialogOption)
        {
            InitializeComponent(true);
            DialogTitle = title;
            Prompt = prompt;
            Input = dialogOption.DefaultValue ?? string.Empty;
            if (dialogOption.Options == null || dialogOption.Options.Length == 0)
            {
                IsText = true;
                Options = dialogOption.Options ?? Array.Empty<string>();
            }
            else
            {
                IsText = false;
                Options = dialogOption.Options;
                Input = Options[0];
            }
            IsOptions = !IsText;
            PasswordChar = dialogOption.IsPassword ? dialogOption.PasswordChar : char.MinValue;
            DataContext = this;
        }
        public string Prompt { get; }
        public char PasswordChar { get; }
        public string DialogTitle { get; }
        public bool IsText { get; }
        public bool IsOptions { get; }
        public string Input { get; set; }
        public string[] Options { get; }



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
