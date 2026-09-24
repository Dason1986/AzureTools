using AzureTools.Services;
using AzureTools.Tools.IO;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolkit.Modules.Comm.ViewModels.Tools
{
    public partial class RsaViewModel : ViewModel
    {
        #region Constructors

        public RsaViewModel([FromKeyedServices(FileGeneratorKeys.Rsa)] IFileGenerator generator, IDialogService dialogService, ILogger<RsaViewModel> logger)
        {
            _generator = generator;
            this.dialogService = dialogService;
            _logger = logger;
        }

        #endregion

        #region Fields & Properties

        private readonly IFileGenerator _generator;
        private readonly IDialogService dialogService;
        private readonly ILogger<RsaViewModel> _logger;
        private IFileEntry[] _fileEntries;

        [ObservableProperty]
        private string _publicKey;

        [ObservableProperty]
        private string _privateKey;

        [ObservableProperty]
        private string _publicKeyBase64;

        [ObservableProperty]
        private string _privateKeyBase64;

        #endregion

        [RelayCommand]
        private void Generate()
        {
            try
            {
                _fileEntries = _generator.Build(new RsaFileOption());

                IFileEntry publicKey = _fileEntries.FirstOrDefault(x => x.Name == "public.pem");
                IFileEntry privateKey = _fileEntries.FirstOrDefault(x => x.Name == "private.pem");

                PublicKey = Encoding.UTF8.GetString(publicKey.GetBuffer());
                PrivateKey = Encoding.UTF8.GetString(privateKey.GetBuffer());
                PublicKeyBase64 = Convert.ToBase64String(publicKey.GetBuffer());
                PrivateKeyBase64 = Convert.ToBase64String(privateKey.GetBuffer());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成RSA密钥对失败");
            }
        }

        public bool CanSave() => _fileEntries != null;

        [RelayCommand]
        private async Task Export()
        {
            if (_fileEntries == null) return;
            IFolderEntry folderEntry = await dialogService.SelectFolderAsync();
            IFileEntry publicKey = _fileEntries.FirstOrDefault(x => x.Name == "public.pem");
            IFileEntry privateKey = _fileEntries.FirstOrDefault(x => x.Name == "private.pem");
            IFileEntry publicKeyPath = folderEntry.CreateFile(publicKey.Name);
            IFileEntry privateKeyPath = folderEntry.CreateFile(privateKey.Name);
            publicKey.SaveAs(publicKeyPath.GetWrite());
            privateKey.SaveAs(privateKeyPath.GetWrite());

        }
    }
}