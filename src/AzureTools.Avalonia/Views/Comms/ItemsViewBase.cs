using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AzureTools.Views.Comms
{
    public abstract class ItemsViewBase : Window
    {
        private readonly Type _itemType;
        private readonly IDictionary<string, string> _propertyMappings;
        private readonly Dictionary<string, PropertyInfo> _properties = new();

        protected DataGrid ItemsDataGrid { get; private set; }

        protected ItemsViewBase(Type itemType, IDictionary<string, string> propertyMappings)
        {
            _itemType = itemType;
            _propertyMappings = propertyMappings;
        }

        protected void InitializeItemsGrid(string name = "ItemsGrid")
        {
            ItemsDataGrid = this.FindControl<DataGrid>(name);
        }

        protected void ConfigureGrid()
        {
            foreach ((string propertyName, PropertyInfo prop, string displayName) in GetDisplayProperties())
            {
                _properties[propertyName] = prop;
                DataGridTemplateColumn col = new DataGridTemplateColumn
                {
                    Header = displayName,
                    CellTemplate = new FuncDataTemplate<object>((item, _) => new TextBlock { Text = GetPropertyValue(item, propertyName) }),
                    Width = DataGridLength.Auto
                };
                ItemsDataGrid.Columns.Add(col);
            }
        }

        protected IReadOnlyList<(string propertyName, PropertyInfo prop, string displayName)> GetDisplayProperties()
        {
            PropertyInfo[] allProps = _itemType.GetProperties();
            List<(string, PropertyInfo, string)> result = new();

            if (_propertyMappings == null || _propertyMappings.Count == 0)
            {
                foreach (PropertyInfo prop in allProps)
                {
                    result.Add((prop.Name, prop, prop.Name));
                }
            }
            else
            {
                foreach (KeyValuePair<string, string> mapping in _propertyMappings)
                {
                    PropertyInfo prop = Array.Find(allProps, p => p.Name.Equals(mapping.Key, StringComparison.OrdinalIgnoreCase));
                    if (prop != null)
                    {
                        result.Add((prop.Name, prop, mapping.Value));
                    }
                }
            }
            return result;
        }

        protected abstract object GetTargetItem(object rowItem);

        private string GetPropertyValue(object item, string propertyName)
        {
            object target = GetTargetItem(item);
            if (target == null) return string.Empty;
            object value = _properties[propertyName].GetValue(target);
            return value?.ToString() ?? string.Empty;
        }
    }
}
