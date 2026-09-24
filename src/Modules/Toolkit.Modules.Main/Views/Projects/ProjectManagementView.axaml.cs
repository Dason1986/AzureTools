using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.Views;
using Toolkit.Modules.Main.ViewModels.Projects;

namespace Toolkit.Modules.Main.Views.Projects
{
    public partial class ProjectManagementView : View
    {
        public ProjectManagementView(ProjectManagementViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
