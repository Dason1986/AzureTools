using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AzureTools.Properties;
using AzureTools.Services.KeyVaults;
using AzureTools.ViewModels;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AzureTools.Views.KeyVaults
{
    public partial class KeyVaultSecretCompareView : View
    {
        private readonly KeyVaultSecretCompareViewModel _viewModel;
        private readonly Dictionary<string, TextBox> _valueTextBoxes = new();
        private readonly DataGrid _dgvResult;
        private readonly StackPanel _valuesPanel;

        public KeyVaultSecretCompareView(KeyVaultSecretCompareViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _dgvResult = this.FindControl<DataGrid>("dgvResult");
            _valuesPanel = this.FindControl<StackPanel>("ValuesPanel");
            CreateDynamicColumns();
            CreateValueTextBoxes();
            _dgvResult.SelectionChanged += OnSelectionChanged;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        [RequiresUnreferencedCode("Calls Avalonia.Data.Binding.Binding()")]
        [RequiresDynamicCode("Calls Avalonia.Data.Binding.Binding()")]
        private void CreateDynamicColumns()
        {
            IValueConverter converter = Resources["CompareResultLookupConverter"] as IValueConverter;
            if (converter == null) return;

            foreach (string envName in _viewModel.EnvNames)
            {
                DataGridTextColumn statusColumn = new DataGridTextColumn
                {
                    Header = string.Format(FormResources.ColEnvStatusFormat, envName),
                    Width = new DataGridLength(100),
                    Binding = new Binding
                    {
                        Path = ".",
                        Converter = converter,
                        ConverterParameter = $"status:{envName}"
                    }
                };
                _dgvResult.Columns.Add(statusColumn);

                DataGridTextColumn valueColumn = new DataGridTextColumn
                {
                    Header = string.Format(FormResources.ColEnvValueFormat, envName),
                    Width = new DataGridLength(200),
                    Binding = new Binding
                    {
                        Path = ".",
                        Converter = converter,
                        ConverterParameter = $"value:{envName}"
                    }
                };
                _dgvResult.Columns.Add(valueColumn);
            }
        }

        private void CreateValueTextBoxes()
        {
            _valuesPanel.Children.Clear();
            _valueTextBoxes.Clear();

            foreach (string envName in _viewModel.EnvNames)
            {
                TextBlock label = new TextBlock
                {
                    Text = envName,
                    FontWeight = FontWeight.Bold,
                    Margin = new Thickness(3, 10, 3, 0)
                };

                TextBox textBox = new TextBox
                {
                    IsReadOnly = true,
                    AcceptsReturn = true,
                    Height = 50,
                    Margin = new Thickness(3, 0, 3, 5)
                };

                _valueTextBoxes[envName] = textBox;
                _valuesPanel.Children.Add(label);
                _valuesPanel.Children.Add(textBox);
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_dgvResult.SelectedItem is not SecretCompareReport result)
            {
                ClearValueTextBoxes();
                return;
            }

            _viewModel.SelectedCompareResult = result;
            UpdateValueTextBoxes(result);
        }

        private void UpdateValueTextBoxes(SecretCompareReport result)
        {
            foreach (KeyValuePair<string, TextBox> kvp in _valueTextBoxes)
            {
                string envName = kvp.Key;
                TextBox textBox = kvp.Value;
                textBox.Text = result.EnvValueMap.TryGetValue(envName, out string value) ? value : "";
            }
        }

        private void ClearValueTextBoxes()
        {
            foreach (TextBox textBox in _valueTextBoxes.Values)
            {
                textBox.Text = "";
            }
        }
    }
}
