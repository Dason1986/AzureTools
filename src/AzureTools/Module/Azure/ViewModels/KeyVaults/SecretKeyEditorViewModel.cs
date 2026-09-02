using AzureTools.Models.KeyVaults;
using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureTools.Module.Azure.ViewModels.KeyVaults
{
    public partial class SecretKeyEditorViewModel : EditorViewModel<SecretKeyOptionItem>
    {
        #region Constructors
        public SecretKeyEditorViewModel(SecretKeyEditorOption option) : base(option)
        {


            if (ExistingItem != null)
            {
                Key = ExistingItem.Key;
                Type = ExistingItem.Type;
                Required = ExistingItem.Required;
                Description = ExistingItem.Description;
                TagsText = ExistingItem.Tags != null ? string.Join(", ", ExistingItem.Tags) : string.Empty;
                LoadValidationOptions(ExistingItem.Option);
            }

            UpdateTypeVisibility();
        }
        #endregion
        #region Fields & Properties
        private static readonly string[] _availableTypes = OptionFactory.Instance.GetSecretKeyTypes();

        [ObservableProperty]
        private string _key;

        [ObservableProperty]
        private string _type;

        [ObservableProperty]
        private bool _required;

        [ObservableProperty]
        private string _description;

        [ObservableProperty]
        private string _tagsText;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _hasMin;

        [ObservableProperty]
        private double _minValue;

        [ObservableProperty]
        private bool _hasMax;

        [ObservableProperty]
        private double _maxValue;

        [ObservableProperty]
        private string _passwordKey;



        [ObservableProperty]
        private bool _isPfxPasswordType;

        [ObservableProperty]
        private bool _isNumberType;

        [ObservableProperty]
        private bool _isDefaultType;


        public IEnumerable<string> AvailableTypes => _availableTypes;


        #endregion


        partial void OnTypeChanged(string value)
        {
            UpdateTypeVisibility();
        }

        private void UpdateTypeVisibility()
        {
            IsPfxPasswordType = Type == "Pfx_Password";
            IsNumberType = Type == "Number";
            IsDefaultType = !IsPfxPasswordType && !IsNumberType;
        }

        private void LoadValidationOptions(ValidationOptions option)
        {
            switch (option)
            {
                case PfxPasswordValidationOptions pfxOpt:
                    PasswordKey = pfxOpt.PasswordKey;
                    ErrorMessage = pfxOpt.ErrorMessage;
                    break;
                case NumberValidationOptions numOpt:
                    ErrorMessage = numOpt.ErrorMessage;
                    IRangeValidationOptions<double> range = numOpt;
                    HasMin = range.Min.HasValue;
                    MinValue = range.Min ?? 0;
                    HasMax = range.Max.HasValue;
                    MaxValue = range.Max ?? 0;
                    break;
                case DefaultValidationOptions defOpt:
                    ErrorMessage = defOpt.ErrorMessage;
                    break;
                default:
                    ErrorMessage = string.Empty;
                    break;
            }
        }



        public SecretKeyOptionItem GetItem()
        {
            string[] tags = TagsText?
                .Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToArray() ?? Array.Empty<string>();

            ValidationOptions option = GetValidationOptions();

            if (IsEditMode)
            {
                ExistingItem.Key = Key?.Trim();
                ExistingItem.Type = Type;
                ExistingItem.Required = Required;
                ExistingItem.Description = Description;
                ExistingItem.Tags = tags;
                ExistingItem.Option = option;
                return ExistingItem;
            }

            return new SecretKeyOptionItem
            {
                Key = Key?.Trim(),
                Type = Type,
                Required = Required,
                Description = Description,
                Tags = tags,
                Option = option
            };
        }

        private ValidationOptions GetValidationOptions()
        {
            if (IsPfxPasswordType)
            {
                return new PfxPasswordValidationOptions
                {
                    Type = "Pfx_Password",
                    PasswordKey = PasswordKey?.Trim(),
                    ErrorMessage = ErrorMessage?.Trim()
                };
            }

            if (IsNumberType)
            {
                NumberValidationOptions options = new NumberValidationOptions
                {
                    Type = "Number",
                    ErrorMessage = ErrorMessage?.Trim()
                };
                IRangeValidationOptions<double> range = options;
                range.Min = HasMin ? MinValue : null;
                range.Max = HasMax ? MaxValue : null;
                return options;
            }

            return new DefaultValidationOptions
            {
                Type = Type,
                ErrorMessage = ErrorMessage?.Trim()
            };
        }
    }
}
