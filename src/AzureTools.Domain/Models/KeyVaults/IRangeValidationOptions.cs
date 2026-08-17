using System;

namespace AzureTools.Models.KeyVaults
{
    public interface IRangeValidationOptions
    {
        object Min { get; set; }
        object Max { get; set; }
    }
    public interface IRangeValidationOptions<T> : IRangeValidationOptions
        where T : struct, IComparable<T>
    {
        new T? Min { get; set; }
        new T? Max { get; set; }
    }
}
