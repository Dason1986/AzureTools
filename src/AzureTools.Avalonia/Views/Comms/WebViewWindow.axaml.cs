using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace AzureTools.Views.Comms
{
    public partial class WebViewWindow : Window
    {
        public WebViewWindow(string url, string title = null)
        {
            InitializeComponent();
            Title = title ?? "Web View";
            NativeWebView webView = this.FindControl<NativeWebView>("WebView");
            webView.Source = new Uri(url);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}