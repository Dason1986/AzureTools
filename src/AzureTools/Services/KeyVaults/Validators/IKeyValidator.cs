namespace AzureTools.Services.KeyVaults.Validators
{
    public interface IKeyValidator
    {
        string Type { get; }
        string ErrorMessage { get; }
        bool Validate(string value, ValidatorContext context);
    }
}
