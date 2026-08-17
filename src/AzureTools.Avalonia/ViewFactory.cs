using AzureTools.ViewModels;
using AzureTools.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace AzureTools
{
    public class ViewFactory : IViewFactory
    {

        public TViewModel CreateViewModel<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>
            (ViewModelContext context, params object[] parameters) where TViewModel : ViewModel
        {
            return ActivatorUtilities.CreateInstance<TViewModel>(context.ServiceProvider, parameters);
        }
        public TView CreateView<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView>
            (ViewModelContext context, params object[] parameters) where TView : View
        {
            return ActivatorUtilities.CreateInstance<TView>(context.ServiceProvider, parameters);
        }


    }
}
