using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.Views;
using Toolkit.Modules.Main.ViewModels.Projects;

namespace Toolkit.Modules.Main.Views.Projects
{
    public partial class ProjectEditorView : View
    {
        public ProjectEditorView(ProjectEditorViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
