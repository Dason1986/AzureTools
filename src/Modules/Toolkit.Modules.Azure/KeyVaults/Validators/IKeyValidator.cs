namespace Toolkit.Modules.Azure.KeyVaults.Validators
{
    public interface IKeyValidator
    {
        string Type { get; }
        string ErrorMessage { get; }
        bool Validate(string value, ValidatorContext context);
    }
}
