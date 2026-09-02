using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AzureTools.Cli
{
    [SupportedOSPlatform("windows")]
    internal static class PtyInterop
    {
        private const string kernel32 = "kernel32.dll";

        public const uint EXTENDED_STARTUPINFO_PRESENT = 0x00080000;
        public const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;
        public const uint CREATE_NO_WINDOW = 0x08000000;
        public const uint WAIT_TIMEOUT = 258;
        public const uint WAIT_INFINITE = 0xFFFFFFFF;
        public static readonly IntPtr PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE = 0x20016;

        [DllImport(kernel32, SetLastError = true)]
        public static extern int CreatePseudoConsole(
            COORD size,
            SafePipeHandle hInput, SafePipeHandle hOutput,
            uint dwFlags, out SafePseudoConsoleHandle phPC);

        [StructLayout(LayoutKind.Sequential)]
        public struct COORD
        {
            public short X;
            public short Y;

            public COORD(short x, short y) { X = x; Y = y; }
        }

        [DllImport(kernel32, SetLastError = true)]
        public static extern int ClosePseudoConsole(IntPtr hPC);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool CreatePipe(
            out SafePipeHandle hReadPipe, out SafePipeHandle hWritePipe,
            IntPtr lpPipeAttributes, uint nSize);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool InitializeProcThreadAttributeList(
            IntPtr lpAttributeList, int dwAttributeCount, int dwFlags, ref IntPtr lpSize);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool UpdateProcThreadAttribute(
            IntPtr lpAttributeList, uint dwFlags,
            IntPtr attribute, IntPtr lpValue, IntPtr cbSize,
            IntPtr lpPreviousValue, IntPtr lpReturnSize);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool DeleteProcThreadAttributeList(IntPtr lpAttributeList);

        [DllImport(kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CreateProcessW(
            string lpApplicationName, string lpCommandLine,
            IntPtr lpProcessAttributes, IntPtr lpThreadAttributes,
            bool bInheritHandles, uint dwCreationFlags,
            IntPtr lpEnvironment, string lpCurrentDirectory,
            ref STARTUPINFOEX lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport(kernel32, SetLastError = true)]
        public static extern uint WaitForSingleObject(SafeWaitHandle hHandle, uint dwMilliseconds);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool TerminateProcess(SafeWaitHandle hProcess, uint uExitCode);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool GetExitCodeProcess(SafeWaitHandle hProcess, out uint lpExitCode);

        [DllImport(kernel32, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFOEX
        {
            public STARTUPINFO StartupInfo;
            public IntPtr lpAttributeList;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX, dwY, dwXSize, dwYSize;
            public int dwXCountChars, dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput, hStdOutput, hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }
    }
}
