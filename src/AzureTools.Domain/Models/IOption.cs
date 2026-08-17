namespace AzureTools.Models
{
    public interface IOption
    {
        string DisplayName { get; }

        bool Disabled { get; set; }
    }
}
