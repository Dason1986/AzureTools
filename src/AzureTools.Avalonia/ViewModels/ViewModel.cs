using Avalonia.Controls;
using Avalonia.VisualTree;
using AzureTools.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace AzureTools.ViewModels
{
    public abstract partial class ViewModel : ObservableObject
    {
        #region Fields & Properties

        [ObservableProperty]
        private string _windowTitle;

        public ContentControl ContentControl { get; private set; }

        public ViewModelContext Context { get; private set; }

        protected virtual Func<Task<bool>> CanClose { get; set; }

        #endregion

        public void Attach(ContentControl contentControl)
        {
            ArgumentNullException.ThrowIfNull(contentControl);
            ContentControl?.DataContext = null;
            ContentControl = contentControl;
            ContentControl.DataContext = this;
        }

        public void SetContext(ViewModelContext context)
        {
            Context = context;
        }

        [RelayCommand]
        protected virtual async Task CloseAsync()
        {
            if (CanClose != null)
            {

                bool result = await CanClose();
                if (result) GetWindow().Close();
                return;
            }
            GetWindow().Close();
        }


        protected async Task<TViewModel> ShowDialog<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>(params object[] viewModelParameters)
     where TViewModel : ViewModel
        {
            CreateInstance(viewModelParameters, out TViewModel viewModel, out View view);
            await view.ShowDialog(GetWindow());

            return viewModel;
        }

        protected TViewModel Show<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>(params object[] viewModelParameters)
       where TViewModel : ViewModel
        {

            CreateInstance(viewModelParameters, out TViewModel viewModel, out View view);

            view.Show(GetWindow());
            return viewModel;
        }

        protected async Task<ViewModel> ShowDialog(Type viewModelType, params object[] viewModelParameters)
        {
            CreateInstance(viewModelParameters, viewModelType, null, out ViewModel viewModel, out View view);
            await view.ShowDialog(GetWindow());

            return viewModel;
        }

        private void CreateInstance(object[] viewModelParameters, Type viewModelType, Type viewType, out ViewModel viewModel, out View view)
        {
            ViewModelContext childContext = CreateChildContext();
            IViewFactory factory = Context.ServiceProvider.GetRequiredService<IViewFactory>();
            IViewContainerManagement containerManagement = Context.ServiceProvider.GetRequiredService<IViewContainerManagement>();
            if (viewType == null) viewType = containerManagement.GetViewType(viewModelType);
            if (viewType == null) throw new Exception("未注入对应的界面");
            ViewModel _viewModel = factory.CreateViewModel(viewModelType, childContext, viewModelParameters);
            View _view = factory.CreateView(viewType, childContext, _viewModel, childContext);
            if (_viewModel.CanClose != null)
            {

                _view.Closing += async (sender, e) =>
                {
                    bool result = await _viewModel.CanClose();
                    if (!result) e.Cancel = true;
                };
            }
            _view.Closed += (s, e) =>
            {
                if (_viewModel is IDisposable disposable) disposable.Dispose();
                childContext.Dispose();
            };
            if (_view.CanMaximize) _view.WindowState = WindowState.Maximized;
            view = _view;
            viewModel = _viewModel;
        }
        private void CreateInstance<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>(object[] viewModelParameters, out TViewModel viewModel, out View view)
    where TViewModel : ViewModel
        {
            CreateInstance(viewModelParameters, typeof(TViewModel), null, out ViewModel _viewModel, out View _view);
            view = _view;
            viewModel = _viewModel is TViewModel viewModel1 ? viewModel1 : null;
        }

        protected ViewModelContext CreateChildContext()
        {
            return Context.CreateChild();
        }

        protected Window GetWindow()
        {
            if (ContentControl is Window window) return window;
            return ContentControl.FindAncestorOfType<Window>();
        }
    }
}
