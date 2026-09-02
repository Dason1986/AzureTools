using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AzureTools.Views.Comms
{
    public partial class MultiSelectItemsView : ItemsViewBase
    {
        private readonly IList _dataSource;
        private readonly HashSet<object> _selectedItems = new();
        private bool _isProgrammaticUpdate;

        public string ViewTitle { get; }
        public ObservableCollection<SelectableItem> SelectableItems { get; private set; }

        public IList SelectedItems => _selectedItems.ToList();

        public MultiSelectItemsView(Type itemType, IList dataSource, string title, IDictionary<string, string> propertyMappings = null)
            : base(itemType, propertyMappings)
        {
            InitializeComponent(true);

            _dataSource = dataSource;
            InitializeItemsGrid();

            ViewTitle = title ?? "选择项目";

            ConfigureGrid();
            LoadData();
        }



        protected override object GetTargetItem(object rowItem) => (rowItem as SelectableItem)?.Item;

        private void LoadData()
        {
            ObservableCollection<SelectableItem> selectableItems = new ObservableCollection<SelectableItem>();
            foreach (object item in _dataSource)
            {
                selectableItems.Add(new SelectableItem { Item = item });
            }
            SelectableItems = selectableItems;
            ItemsDataGrid.ItemsSource = selectableItems;
        }

        private void OnItemCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_isProgrammaticUpdate) return;
            if (sender is not CheckBox checkBox) return;
            if (checkBox.DataContext is SelectableItem selectable && selectable.Item != null)
            {
                if (selectable.IsSelected)
                    _selectedItems.Add(selectable.Item);
                else
                    _selectedItems.Remove(selectable.Item);
            }
        }

        private void OnSelectAllClick(object sender, RoutedEventArgs e)
        {
            _isProgrammaticUpdate = true;
            _selectedItems.Clear();
            foreach (SelectableItem selectable in SelectableItems)
            {
                selectable.IsSelected = true;
                if (selectable.Item != null)
                {
                    _selectedItems.Add(selectable.Item);
                }
            }
            _isProgrammaticUpdate = false;
        }

        private void OnDeselectAllClick(object sender, RoutedEventArgs e)
        {
            _isProgrammaticUpdate = true;
            foreach (SelectableItem selectable in SelectableItems)
            {
                selectable.IsSelected = false;
            }
            _selectedItems.Clear();
            _isProgrammaticUpdate = false;
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            _selectedItems.Clear();
            Close(false);
        }

        public IList<T> GetSelectedItems<T>()
        {
            List<T> result = new List<T>();
            foreach (object item in _selectedItems)
            {
                if (item is T typed)
                {
                    result.Add(typed);
                }
            }
            return result;
        }
    }
}
