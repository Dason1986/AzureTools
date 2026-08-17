namespace AzureTools.Tools.IO
{
    public interface IFileGenerator
    {
        IFileEntry[] Build(IFileGeneratorOption option);
    }
}
