using AzureTools.Services;
using AzureTools.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Toolkit.Modules.Models;
using Toolkit.Modules.Properties;
using Toolkit.Modules.Stores;

namespace Toolkit.Modules.Main.ViewModels.Projects
{
    public partial class ProjectManagementViewModel : ManageViewModel<ProjectInfo>
    {
        #region Constructors

        public ProjectManagementViewModel(IToolkitStore store, IDialogService dialogService) : base(dialogService)
        {
            _store = store;

            LoadProjects();
        }

        #endregion

        #region Fields & Properties

        private readonly IToolkitStore _store;
        private readonly IDialogService _dialogService;

        #endregion

        private void LoadProjects()
        {
            ProjectInfo[] projects = _store.GetProjects();
            Items = new ObservableCollection<ProjectInfo>(projects);
        }

        protected override async Task AddAsync()
        {
            ProjectInfo project = await EditOptionAsync(null);
            if (project == null) return;
            AddOrUpdateProject(project);
        }

        protected override async Task EditAsync()
        {
            if (SelectedItem == null)
            {
                await _dialogService.ShowInfoAsync(MessageResources.SelectItemToEdit);
                return;
            }
            ProjectInfo project = await EditOptionAsync(SelectedItem);
            if (project == null) return;
            AddOrUpdateProject(project);
        }

        protected override async Task<ProjectInfo> EditOptionAsync(ProjectInfo project)
        {

            ProjectEditorViewModel editViewModel = await ShowDialog<ProjectEditorViewModel>(new ProjectEditorOption { Option = project, IsEditMode = project != null });
            return editViewModel.DialogResult == true ? editViewModel.EditedProject : null;
        }

        protected override async Task DeleteAsync()
        {
            if (SelectedItem == null)
            {
                await _dialogService.ShowInfoAsync(MessageResources.SelectItemToDelete);
                return;
            }
            bool confirm = await _dialogService.ShowConfirmAsync(string.Format(FormResources.MsgConfirmDeleteProject, SelectedItem.Name));
            if (!confirm) return;
            _store.DeleteProject(SelectedItem);
            _store.Save();
            LoadProjects();
        }

        protected override Task RefreshAsync()
        {
            LoadProjects();
            return Task.CompletedTask;
        }

        private void AddOrUpdateProject(ProjectInfo project)
        {
            if (project == null) return;
            if (project.Id != Guid.Empty)
            {
                _store.UpdateProject(project);
            }
            else
            {
                _store.AddProject(project);
            }
            _store.Save();
            LoadProjects();
        }

        protected override Task SaveAsync()
        {
            throw new NotImplementedException();
        }

        protected override Task DiscardChangesAsync()
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> HandleCloseAsync()
        {
            throw new NotImplementedException();
        }
    }
}