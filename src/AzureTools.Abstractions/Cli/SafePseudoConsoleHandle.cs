using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.Versioning;
using System.Threading;

namespace AzureTools.Cli
{
    [SupportedOSPlatform("windows")]
    internal sealed class SafePseudoConsoleHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafePseudoConsoleHandle() : base(true) { }

        public SafePseudoConsoleHandle(IntPtr existingHandle, bool ownsHandle) : base(true)
        {
            SetHandle(existingHandle);
            if (!ownsHandle) SetHandleAsInvalid();
        }

        private int _closed;

        protected override bool ReleaseHandle()
        {
            if (Interlocked.Exchange(ref _closed, 1) != 0) return true;
            return PtyInterop.ClosePseudoConsole(handle) == 0;
        }
    }
}
