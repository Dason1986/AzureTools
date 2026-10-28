namespace Toolkit.Modules.SSH.Models
{
    public class FirewallRule
    {
        public string Number { get; set; }

        public string Port { get; set; }

        public string Protocol { get; set; }

        public string Action { get; set; }

        public string Direction { get; set; }

        public string Source { get; set; }

        public string Raw { get; set; }
    }
}
