using System.Collections.Generic;
using System.Linq;

namespace Toolkit.Modules.Azure.KeyVaults
{
    public class SecretCompareReport
    {
        public string KeyName { get; set; }

        public CompareStatus Status { get; set; }

        public Dictionary<string, string> EnvStatusMap { get; set; } = new();

        public Dictionary<string, string> EnvValueMap { get; set; } = new();



        public bool GetIsSame()
        {
            List<string> values = EnvValueMap.Values.Where(v => v != null).ToList();
            if (values.Count < 2) return false;
            return values.All(v => string.Equals(v, values[0]));
        }
    }
}
