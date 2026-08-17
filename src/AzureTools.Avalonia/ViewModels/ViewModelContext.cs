using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace AzureTools.ViewModels
{
    public class ViewModelContext : System.IDisposable
    {
        public IServiceProvider ServiceProvider { get; }


        public IDictionary<string, object> Items { get; }

        private IServiceScope Scope { get; set; }

        public ViewModelContext(IServiceProvider serviceProvider, IDictionary<string, object> items = null)
        {
            ServiceProvider = serviceProvider;

            Items = items ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public ViewModelContext CreateChild()
        {
            IServiceScope scope = ServiceProvider.CreateScope();
            return new ViewModelContext(scope.ServiceProvider) { Scope = scope };
        }

        public void Dispose()
        {
            Scope?.Dispose();
        }
    }
}
