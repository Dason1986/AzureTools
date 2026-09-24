using System;
using System.Diagnostics.CodeAnalysis;

namespace AzureTools
{
    readonly struct KeyedViewMapping
    {
        public KeyedViewMapping(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type viewModelType,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type viewType,
            KeyedViewModelFactory viewModelFactory,
            KeyedViewFactory viewFactory)
        {
            ViewModelType = viewModelType;
            ViewType = viewType;
            ViewModelFactory = viewModelFactory;
            ViewFactory = viewFactory;
        }

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        public Type ViewModelType { get; }

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        public Type ViewType { get; }

        public KeyedViewModelFactory ViewModelFactory { get; }

        public KeyedViewFactory ViewFactory { get; }
    }
}
