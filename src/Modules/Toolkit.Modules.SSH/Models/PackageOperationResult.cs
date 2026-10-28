namespace Toolkit.Modules.SSH.Models
{
    public class PackageOperationResult
    {
        public string Command { get; set; }

        public string Output { get; set; }

        public string Error { get; set; }

        public int ExitStatus { get; set; }

        public bool Success { get; set; }
    }
}
