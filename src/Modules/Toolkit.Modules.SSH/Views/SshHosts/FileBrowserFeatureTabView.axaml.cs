using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Toolkit.Modules.SSH.Views.SshHosts
{
    public partial class FileBrowserFeatureTabView : UserControl
    {
        public FileBrowserFeatureTabView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
