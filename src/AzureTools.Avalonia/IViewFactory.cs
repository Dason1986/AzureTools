using AzureTools.ViewModels;
using AzureTools.Views;
using System;
using System.Diagnostics.CodeAnalysis;

namespace AzureTools
{
    public interface IViewFactory
    {
        TViewModel CreateViewModel<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>(ViewModelContext context, params object[] parameters)
            where TViewModel : ViewModel;
        ViewModel CreateViewModel(Type viewModelType, ViewModelContext context, params object[] parameters);
        TView CreateView<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView>(ViewModelContext context, params object[] parameters)
            where TView : View;
        View CreateView(Type viewType, ViewModelContext context, params object[] parameters);
    }
}
