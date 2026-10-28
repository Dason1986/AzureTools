namespace Toolkit.Modules.SSH.ContainerRuntime.Models
{
    public class Volume
    {
        public string Name { get; set; }
        public string Driver { get; set; }
        public string Mountpoint { get; set; }
        public string Scope { get; set; }
    }
}
