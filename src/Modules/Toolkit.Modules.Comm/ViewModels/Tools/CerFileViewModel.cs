using AzureTools.Services;
using AzureTools.Tools.IO;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Toolkit.Modules.Properties;

namespace Toolkit.Modules.Comm.ViewModels.Tools
{
    public partial class CerFileViewModel : ViewModel
    {
        #region Constructors

        public CerFileViewModel([FromKeyedServices(FileGeneratorKeys.Cer)] IFileGenerator generator, IDialogService dialogService, ILogger<CerFileViewModel> logger)
        {
            _generator = generator;
            this.dialogService = dialogService;
            _logger = logger;
        }

        #endregion

        #region Fields & Properties

        private readonly IFileGenerator _generator;
        private readonly IDialogService dialogService;
        private readonly ILogger<CerFileViewModel> _logger;
        private IFileEntry[] _fileEntries;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private int _year = 2;

        [ObservableProperty]
        private string _cerBase64;

        [ObservableProperty]
        private string _pfxBase64;

        [ObservableProperty]
        private bool _hasErrors;

        [ObservableProperty]
        private string _errorMessage;

        #endregion

        [RelayCommand]
        private void Generate()
        {
            HasErrors = false;
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Name))
            {
                HasErrors = true;
                ErrorMessage = MessageResources.CertNameEmpty;
                return;
            }


            try
            {
                HasErrors = false;
                ErrorMessage = "";
                _fileEntries = _generator.Build(new CerFileOption
                {
                    Name = Name,
                    Password = Password,
                    Year = Year
                });

                IFileEntry pfx = _fileEntries.FirstOrDefault(x => x.Name.EndsWith(".pfx"));
                IFileEntry cer = _fileEntries.FirstOrDefault(x => x.Name.EndsWith(".cer"));

                CerBase64 = Convert.ToBase64String(cer.GetBuffer());
                PfxBase64 = Convert.ToBase64String(pfx.GetBuffer());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成证书失败，Name：{Name}，Year：{Year}", Name, Year);
                HasErrors = true;
                ErrorMessage = ex.Message;
            }
        }

        public bool CanSave() => _fileEntries != null;

        [RelayCommand]
        private async Task ExportAsync()
        {
            if (_fileEntries == null) return;
            IFolderEntry folderEntry = await dialogService.SelectFolderAsync();
            IFileEntry pfx = _fileEntries.FirstOrDefault(x => x.Name.EndsWith(".pfx"));
            IFileEntry cer = _fileEntries.FirstOrDefault(x => x.Name.EndsWith(".cer"));
            IFileEntry pfxPath = folderEntry.CreateFile(pfx.Name);
            IFileEntry cerPath = folderEntry.CreateFile(cer.Name);
            pfx.SaveAs(pfxPath.GetWrite());
            cer.SaveAs(cerPath.GetWrite());

        }
    }
}