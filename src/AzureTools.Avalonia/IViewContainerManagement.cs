using System;
using System.Diagnostics.CodeAnalysis;

namespace AzureTools
{
    public interface IViewContainerManagement
    {
        Type GetViewType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>();
        Type GetViewType(Type viewModelType);
        void AddView<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView>();
        void AddKeyedView<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView>(string key, KeyedViewModelFactory viewModelFactory, KeyedViewFactory viewFactory);
        Type GetViewModelTypeByKey(string key);
        Type GetViewTypeByKey(string key);
        KeyedViewModelFactory GetViewModelFactoryByKey(string key);
        KeyedViewFactory GetViewFactoryByKey(string key);
    }

    public class DefaultViewContainerManagement : IViewContainerManagement
    {
        private readonly System.Collections.Concurrent.ConcurrentDictionary<Type, Type> keyValues = new();

        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, KeyedViewMapping> keyedViews;



        internal DefaultViewContainerManagement(
            System.Collections.Concurrent.ConcurrentDictionary<Type, Type> mapping,
            System.Collections.Concurrent.ConcurrentDictionary<string, KeyedViewMapping> keyedMapping)
        {
            keyValues = mapping;
            keyedViews = keyedMapping;
        }

        public void AddView<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView>()
        {
            keyValues.TryAdd(typeof(TViewModel), typeof(TView));
        }

        public void AddKeyedView<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView>(string key, KeyedViewModelFactory viewModelFactory, KeyedViewFactory viewFactory)
        {
            if (!keyedViews.TryAdd(key, new KeyedViewMapping(typeof(TViewModel), typeof(TView), viewModelFactory, viewFactory)))
                throw new InvalidOperationException($"已存在相同的视图键：{key}");
        }

        public Type GetViewType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>()
        {
            return GetViewType(typeof(TViewModel));
        }
        public Type GetViewType(Type viewModelType)
        {
            return keyValues.TryGetValue(viewModelType, out Type viewType) ? viewType : null;
        }

        public Type GetViewModelTypeByKey(string key)
        {
            return keyedViews.TryGetValue(key, out KeyedViewMapping mapping) ? mapping.ViewModelType : null;
        }

        public Type GetViewTypeByKey(string key)
        {
            return keyedViews.TryGetValue(key, out KeyedViewMapping mapping) ? mapping.ViewType : null;
        }

        public KeyedViewModelFactory GetViewModelFactoryByKey(string key)
        {
            return keyedViews.TryGetValue(key, out KeyedViewMapping mapping) ? mapping.ViewModelFactory : null;
        }

        public KeyedViewFactory GetViewFactoryByKey(string key)
        {
            return keyedViews.TryGetValue(key, out KeyedViewMapping mapping) ? mapping.ViewFactory : null;
        }
    }
}
