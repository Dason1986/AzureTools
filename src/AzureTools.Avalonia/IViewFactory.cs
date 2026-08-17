using AzureTools.ViewModels;
using AzureTools.Views;
using System.Diagnostics.CodeAnalysis;

namespace AzureTools
{
    public interface IViewFactory
    {
        TViewModel CreateViewModel<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>(ViewModelContext context, params object[] parameters)
            where TViewModel : ViewModel;

        TView CreateView<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView>(ViewModelContext context, params object[] parameters)
            where TView : View;

    }
}
