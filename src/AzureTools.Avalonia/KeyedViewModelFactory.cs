using AzureTools.ViewModels;
using System;

namespace AzureTools
{
    public delegate ViewModel KeyedViewModelFactory(ViewParameters parameters, IServiceProvider serviceProvider);
}
