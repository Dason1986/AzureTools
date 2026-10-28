using System.Text.RegularExpressions;

namespace AzureTools.Cli
{
    public static class AnsiEscapeCleaner
    {
        private static readonly Regex Pattern = new(
            @"\u001B(\][^\u0007]*(\u0007|\u001B\\)|[()][AB012]|\[[0-?]*[ -/]*[@-~]|[=>Nc])",
            RegexOptions.Compiled);

        public static string Strip(string text) => Pattern.Replace(text, string.Empty);
    }
}
