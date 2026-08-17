using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace AzureTools.Services.Platform
{
    [SupportedOSPlatform("windows")]
    public class WindowsMessageFallback : IMessageFallback
    {
        private const uint MB_OK = 0x00000000;
        private const uint MB_ICONERROR = 0x00000010;
        private const uint MB_ICONWARNING = 0x00000030;
        private const uint MB_ICONINFORMATION = 0x00000040;
        private const uint CF_UNICODETEXT = 13;
        private const uint GMEM_MOVEABLE = 0x0002;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool OpenClipboard(IntPtr hWndOwner);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetClipboardData(uint format, IntPtr data);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalAlloc(uint flags, IntPtr size);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GlobalUnlock(IntPtr hMem);

        public bool IsSupported => OperatingSystem.IsWindows();

        /// <summary>
        /// 调用 Win32 MessageBox 显示原生消息框。
        /// </summary>
        public void ShowMessage(string message, string title, string iconType)
        {
            uint type = MB_OK | iconType switch
            {
                "Error" => MB_ICONERROR,
                "Warning" => MB_ICONWARNING,
                _ => MB_ICONINFORMATION
            };
            MessageBox(IntPtr.Zero, message ?? string.Empty, title ?? string.Empty, type);
        }

        /// <summary>
        /// 调用 Win32 剪贴板 API（OpenClipboard/EmptyClipboard/SetClipboardData）写入 Unicode 文本。
        /// </summary>
        public bool SetClipboard(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            if (!OpenClipboard(IntPtr.Zero)) return false;

            IntPtr hGlobal = IntPtr.Zero;
            try
            {
                EmptyClipboard();
                byte[] bytes = Encoding.Unicode.GetBytes(text + "\0");
                hGlobal = GlobalAlloc(GMEM_MOVEABLE, bytes.Length);
                if (hGlobal == IntPtr.Zero) return false;

                IntPtr pGlobal = GlobalLock(hGlobal);
                if (pGlobal == IntPtr.Zero) return false;

                Marshal.Copy(bytes, 0, pGlobal, bytes.Length);
                GlobalUnlock(hGlobal);

                IntPtr result = SetClipboardData(CF_UNICODETEXT, hGlobal);
                if (result != IntPtr.Zero) hGlobal = IntPtr.Zero;
                return result != IntPtr.Zero;
            }
            finally
            {
                CloseClipboard();
                if (hGlobal != IntPtr.Zero) Marshal.FreeHGlobal(hGlobal);
            }
        }
    }
}
