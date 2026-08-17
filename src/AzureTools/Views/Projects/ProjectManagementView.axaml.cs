using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.ViewModels.Projects;

namespace AzureTools.Views.Projects
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
