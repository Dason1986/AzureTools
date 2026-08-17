using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace AzureTools.Views
{
    public partial class MessageBoxDialog : Window
    {
        public string Message { get; }
        public string DialogTitle { get; }

        public MessageBoxDialog(string message, string title, string iconType)
        {
            InitializeComponent();
            Message = message;
            DialogTitle = title;
            DataContext = this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
