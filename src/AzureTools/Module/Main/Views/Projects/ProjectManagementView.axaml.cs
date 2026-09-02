using Avalonia.Markup.Xaml;
using AzureTools.Module.Main.ViewModels.Projects;
using AzureTools.ViewModels;
using AzureTools.Views;

namespace AzureTools.Module.Main.Views.Projects
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
