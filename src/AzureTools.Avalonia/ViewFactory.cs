using AzureTools.ViewModels;
using AzureTools.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AzureTools
{
    public class ViewFactory : IViewFactory
    {

        ConcurrentDictionary<Type, ObjectFactory> cache = new();
        public TViewModel CreateViewModel<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>
            (ViewModelContext context, params object[] parameters) where TViewModel : ViewModel
        {

            object instance = CreateViewModel(typeof(TViewModel), context, parameters);

            return instance is TViewModel viewModel ? viewModel : default;
        }
        public ViewModel CreateViewModel(Type viewModelType, ViewModelContext context, params object[] parameters)
        {
            ObjectFactory objectFactory = cache.GetOrAdd(viewModelType, x => ActivatorUtilities.CreateFactory(viewModelType, parameters.Select(x => x.GetType()).ToArray()));
            object instance = objectFactory(context.ServiceProvider, parameters);

            return instance is ViewModel viewModel ? viewModel : default;
        }
        public TView CreateView<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView>
            (ViewModelContext context, params object[] parameters) where TView : View
        {
            object instance = CreateView(typeof(TView), context, parameters);

            return instance is TView view ? view : default;
        }

        public View CreateView(Type viewType, ViewModelContext context, params object[] parameters)
        {
            ObjectFactory objectFactory = cache.GetOrAdd(viewType, x => ActivatorUtilities.CreateFactory(viewType, parameters.Select(x => x.GetType()).ToArray()));
            object instance = objectFactory(context.ServiceProvider, parameters);

            return instance is View view ? view : default;
        }
    }
}
