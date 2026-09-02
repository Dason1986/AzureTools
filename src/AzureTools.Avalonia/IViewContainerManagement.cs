using System;
using System.Diagnostics.CodeAnalysis;

namespace AzureTools
{
    public interface IViewContainerManagement
    {
        Type GetViewType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>();
        Type GetViewType(Type viewModelType);
        void AddView<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView>();
    }

    public class DefaultViewContainerManagement : IViewContainerManagement
    {
        private readonly System.Collections.Concurrent.ConcurrentDictionary<Type, Type> keyValues = new();

        public DefaultViewContainerManagement() { }

        internal DefaultViewContainerManagement(System.Collections.Concurrent.ConcurrentDictionary<Type, Type> mapping)
        {
            keyValues = mapping;
        }

        public void AddView<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView>()
        {
            keyValues.TryAdd(typeof(TViewModel), typeof(TView));
        }

        public Type GetViewType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>()
        {
            return GetViewType(typeof(TViewModel));
        }
        public Type GetViewType(Type viewModelType)
        {
            return keyValues.TryGetValue(viewModelType, out Type viewType) ? viewType : null;
        }
    }
}
