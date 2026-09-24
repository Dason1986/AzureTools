namespace Toolkit.Modules.Azure.Models.KeyVaults
{
    public class NumberValidationOptions : ValidationOptions, IRangeValidationOptions<double>
    {
        #region Constructors

        #endregion

        #region Fields & Properties

        public double? Min { get; set; }

        public double? Max { get; set; }

        object IRangeValidationOptions.Min
        {
            get => Min;
            set => Min = (double?)value;
        }

        object IRangeValidationOptions.Max
        {
            get => Max;
            set => Max = (double?)value;
        }

        #endregion
    }
}
