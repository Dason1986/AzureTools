using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Toolkit.Modules.SSH.Views.ContainerRuntime.Tabs
{
    public partial class VolumesTabView : UserControl
    {
        public VolumesTabView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
