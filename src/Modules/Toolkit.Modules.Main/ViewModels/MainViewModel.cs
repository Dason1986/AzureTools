
using AzureTools;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Toolkit.Modules.Main.ViewModels
{
    public partial class MainViewModel : ViewModel
    {
        #region Constructors

        public MainViewModel(IMenuManagement menuManagement)
        {
            this.menuManagement = menuManagement;
            LoadMenus();
        }

        #endregion

        #region Fields & Properties




        public ObservableCollection<MenuModule> Modules { get; } = new();

        [ObservableProperty]
        private MenuModule _selectedModule;
        private readonly IMenuManagement menuManagement;

        public string CurrentModuleName => SelectedModule?.Name ?? string.Empty;

        partial void OnSelectedModuleChanged(MenuModule value)
        {
            OnPropertyChanged(nameof(CurrentModuleName));
        }

        #endregion



        private MenuItemCard CreateCard(IMenuItem menu)
        {
            return new MenuItemCard
            {
                Menu = menu,
                ClickCommand = new RelayCommand(async () => await OpenMenu(menu))
            };
        }

        private void LoadMenus()
        {
            Modules.Clear();
            IOrderedEnumerable<MenuModule> arr = menuManagement.GetMenus().GroupBy(menu => menu.Module.Code).Select(group =>
            {
                IModuleItem module = group.First().Module;
                return new MenuModule
                {
                    Code = module.Code,
                    Name = module.Name,
                    Order = module.Order,
                    Cards = group.Select(CreateCard).ToArray()
                };
            }).OrderByDescending(x => x.Order);
            foreach (MenuModule item in arr)
            {
                Modules.Add(item);
            }

            SelectedModule = Modules.FirstOrDefault();
        }

        private async Task OpenMenu(IMenuItem menu)
        {
            if (menu.ViewModelType == null) return;
            await ShowDialog(menu.ViewModelType);
        }
    }
}
