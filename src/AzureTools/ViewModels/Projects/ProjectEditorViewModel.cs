using AzureTools.Models;
using AzureTools.Properties;
using AzureTools.Services;
using AzureTools.Stores;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureTools.ViewModels.Projects
{
    public partial class ProjectEditorViewModel : DialogViewModel
    {
        #region Constructors

        public ProjectEditorViewModel(ProjectEditorOption option, IAzureToolsStore store, IDialogService dialogService)
        {
            _store = store;
            _dialogService = dialogService;
            IsEditMode = option.IsEditMode;
            _project = option.Option;
            LoadProject();
        }

        #endregion

        #region Fields & Properties

        private readonly IAzureToolsStore _store;
        private readonly IDialogService _dialogService;
        private readonly ProjectInfo _project;
        public bool IsEditMode { get; }

        [ObservableProperty]
        private string _code;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _description;

        public ProjectInfo EditedProject { get; private set; }



        public bool? DialogResult { get; private set; }

        #endregion

        void LoadProject()
        {
            if (_project == null) return;

            Code = _project.Code;
            Name = _project.Name;
            Description = _project.Description;
        }


        protected override async Task ConfirmAsync()
        {
            List<string> errors = ValidateInput();
            if (errors.Count > 0)
            {
                await _dialogService.ShowInfoAsync(string.Join("\r\n", errors));
                return;
            }
            EditedProject = CreateProjectFromForm();
            DialogResult = true;
            GetWindow().Close();
        }

       
        private List<string> ValidateInput()
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrWhiteSpace(Code)) errors.Add(FormResources.LblProjectCode);
            if (string.IsNullOrWhiteSpace(Name)) errors.Add(FormResources.LblProjectName);

            if (!IsEditMode && _store.ExistProject(Code))
            {
                errors.Add(FormResources.LblProjectCodeExist);
            }
            if (IsEditMode && _store.ExistProject(_project.Id, Code))
            {
                errors.Add(FormResources.LblProjectCodeExist);
            }
            return errors;
        }

        private ProjectInfo CreateProjectFromForm()
        {
            if (IsEditMode)
            {
                _project.Code = Code;
                _project.Name = Name;
                _project.Description = Description;
                return _project;
            }

            return new ProjectInfo
            {
                Code = Code,
                Name = Name,
                Description = Description
            };
        }
    }
}