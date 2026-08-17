using System;

namespace AzureTools.SDK
{
    public class AzureToolsException : Exception
    {
        public AzureToolsException() { }
        public AzureToolsException(string message) : base(message) { }
        public AzureToolsException(string message, Exception inner) : base(message, inner) { }
    }
}
