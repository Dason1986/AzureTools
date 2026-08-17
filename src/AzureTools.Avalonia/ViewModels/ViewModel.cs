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
        private async Task CloseAsync()
        {
            if (CanClose != null)
            {

                bool result = await CanClose();
                if (result) GetWindow().Close();
                return;
            }
            GetWindow().Close();
        }


        protected async Task<TViewModel> ShowDialog<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>(params object[] viewModelParameters)
            where TView : View
            where TViewModel : ViewModel
        {
            ViewModelContext childContext = CreateChildContext();
            IViewFactory factory = Context.ServiceProvider.GetRequiredService<IViewFactory>();
            TViewModel viewModel = factory.CreateViewModel<TViewModel>(childContext, viewModelParameters);
            TView view = factory.CreateView<TView>(childContext, viewModel, childContext);
            if (viewModel.CanClose != null)
            {

                view.Closing += async (sender, e) =>
                {
                    bool result = await viewModel.CanClose();
                    if (!result) e.Cancel = true;
                };
            }
            await view.ShowDialog(GetWindow());
            childContext.Dispose();
            return viewModel;
        }



        protected void Show<TView>(TView view) where TView : View
        {
            view.Closed += (s, e) => view.Context?.Dispose();
            view.Show(GetWindow());
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
