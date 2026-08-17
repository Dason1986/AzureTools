using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace AzureTools.Views
{
    public partial class ConfirmDialog : Window
    {
        public string Message { get; }
        public string DialogTitle { get; }

        public ConfirmDialog(string message, string title)
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

        private void OnYesClick(object sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void OnNoClick(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
