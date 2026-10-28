using System.Collections.Generic;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.Managers
{
    public interface ISshConnectionTestService
    {
        Task<IDictionary<string, string>> GetProfileAsync(SshHostProfile host);
    }
}
