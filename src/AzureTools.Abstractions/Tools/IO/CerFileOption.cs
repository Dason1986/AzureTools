namespace AzureTools.Tools.IO
{

    public class CerFileOption : IFileGeneratorOption
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public int Year { get; set; } = 2;
    }
}
