using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AzureTools.Views.Comms
{
    public partial class SelectItemsView : ItemsViewBase
    {
        private readonly IList _dataSource;

        public string ViewTitle { get; }
        public IList Items { get; private set; }
        public object SelectedItem { get; private set; }

        public SelectItemsView(Type itemType, IList dataSource, string title, IDictionary<string, string> propertyMappings = null)
            : base(itemType, propertyMappings)
        {
            InitializeComponent();

            _dataSource = dataSource;
            InitializeItemsGrid();

            ViewTitle = title ?? "选择项目";

            ConfigureGrid();
            LoadData();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override object GetTargetItem(object rowItem) => rowItem;

        private void LoadData()
        {
            List<object> items = new List<object>();
            foreach (object item in _dataSource)
            {
                items.Add(item);
            }
            Items = items;
            ItemsDataGrid.ItemsSource = items;
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            SelectedItem = ItemsDataGrid.SelectedItem;
            Close(true);
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            SelectedItem = null;
            Close(false);
        }

        public T GetSelectedItem<T>()
        {
            return (T)SelectedItem;
        }
    }
}
