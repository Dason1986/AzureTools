using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Toolkit.Modules.SSH.Views.ContainerRuntime.Tabs
{
    public partial class ContainersTabView : UserControl
    {
        public ContainersTabView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
