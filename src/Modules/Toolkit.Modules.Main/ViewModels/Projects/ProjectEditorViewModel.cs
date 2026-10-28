using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toolkit.Modules.Main.Properties;
using Toolkit.Modules.Models;
using Toolkit.Modules.Stores;

namespace Toolkit.Modules.Main.ViewModels.Projects
{
    public partial class ProjectEditorViewModel : DialogViewModel
    {
        #region Constructors

        public ProjectEditorViewModel(ProjectEditorOption option, IToolkitStore store, IDialogService dialogService)
        {
            _store = store;
            _dialogService = dialogService;
            IsEditMode = option.IsEditMode;
            _project = option.Option;
            LoadProject();
        }

        #endregion

        #region Fields & Properties

        private readonly IToolkitStore _store;
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
            if (string.IsNullOrWhiteSpace(Code)) errors.Add(MainResource.LblProjectCode);
            if (string.IsNullOrWhiteSpace(Name)) errors.Add(MainResource.LblProjectName);

            if (!IsEditMode && _store.ExistProject(Code))
            {
                errors.Add(MainResource.LblProjectCodeExist);
            }
            if (IsEditMode && _store.ExistProject(_project.Id, Code))
            {
                errors.Add(MainResource.LblProjectCodeExist);
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