using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Toolkit.Modules.SSH.Views.SshHosts
{
    public partial class DockerFeatureTabView : UserControl
    {
        public DockerFeatureTabView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
