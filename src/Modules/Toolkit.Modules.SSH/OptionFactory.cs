using Toolkit.Modules.Models;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH
{
    class OptionFactory
    {
        public static readonly OptionFactory Instance = new OptionFactory();
        public Option<SshAuthMode>[] AuthModeOptions { get; } =
         [
             new Option<SshAuthMode>(SshAuthMode.Password, "密码"),
             new Option<SshAuthMode>(SshAuthMode.PrivateKey, "私钥")
         ];
    }
}
