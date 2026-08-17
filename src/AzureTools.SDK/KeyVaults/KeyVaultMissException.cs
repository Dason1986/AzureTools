using System;
using System.Collections.Generic;

namespace AzureTools.SDK.KeyVaults
{
    public class KeyVaultMissException : AzureToolsException
    {
        public KeyVaultMissException() { }

        public KeyVaultMissException(List<string> misskeys)
        {
            MissKeys = misskeys;
        }

        public KeyVaultMissException(string message, Exception inner) : base(message, inner) { }

        public List<string> MissKeys { get; }
    }
}
