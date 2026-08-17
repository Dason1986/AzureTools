using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AzureTools.SDK.Models;

namespace AzureTools.Behaviors
{
    public static class DataGridAssist
    {
        public static readonly AttachedProperty<bool> EnableStoppedRowHighlightProperty =
            AvaloniaProperty.RegisterAttached<DataGrid, bool>("EnableStoppedRowHighlight", typeof(DataGridAssist));

        public static readonly AttachedProperty<string> HighlightStatusValueProperty =
            AvaloniaProperty.RegisterAttached<DataGrid, string>("HighlightStatusValue", typeof(DataGridAssist), "");

        public static bool GetEnableStoppedRowHighlight(DataGrid element)
        {
            return element.GetValue(EnableStoppedRowHighlightProperty);
        }

        public static void SetEnableStoppedRowHighlight(DataGrid element, bool value)
        {
            element.SetValue(EnableStoppedRowHighlightProperty, value);
            AttachHandler(element, value);
        }

        public static string GetHighlightStatusValue(DataGrid element)
        {
            return element.GetValue(HighlightStatusValueProperty);
        }

        public static void SetHighlightStatusValue(DataGrid element, string value)
        {
            element.SetValue(HighlightStatusValueProperty, value);
        }

        private static void AttachHandler(DataGrid dataGrid, bool enable)
        {
            if (enable)
            {
                dataGrid.LoadingRow -= OnLoadingRow;
                dataGrid.LoadingRow += OnLoadingRow;
            }
            else
            {
                dataGrid.LoadingRow -= OnLoadingRow;
            }
        }

        private static void OnLoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (sender is not DataGrid dataGrid) return;
            string targetValue = GetHighlightStatusValue(dataGrid);

            if (e.Row.DataContext is IStatus info && info.Status == targetValue)
            {
                e.Row.Background = Brushes.LightPink;
            }
            else
            {
                e.Row.Background = Brushes.Transparent;
            }
        }
    }
}
