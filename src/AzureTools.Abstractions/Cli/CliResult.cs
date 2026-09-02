namespace AzureTools.Cli
{
    public class CliResult
    {
        public CliResult(string output, string error, int exitCode)
        {
            Output = output;
            Error = error;
            ExitCode = exitCode;
        }
        public string Output { get; }
        public string Error { get; }
        public int ExitCode { get; }
        public bool Success => ExitCode == 0;
    }

}
