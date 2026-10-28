using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Toolkit.Modules.SSH.Views.SshHosts
{
    public partial class PodmanFeatureTabView : UserControl
    {
        public PodmanFeatureTabView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
