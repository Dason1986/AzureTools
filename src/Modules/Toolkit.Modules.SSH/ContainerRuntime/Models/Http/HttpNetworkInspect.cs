using System.Collections.Generic;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models.Http
{
    public class HttpNetworkInspect
    {
        public string Name { get; set; }
        public Dictionary<string, HttpNetworkEndpoint> Containers { get; set; }
    }
}
