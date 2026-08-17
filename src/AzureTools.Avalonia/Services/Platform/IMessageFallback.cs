namespace AzureTools.Services.Platform
{
    /// <summary>
    /// UI 线程不可用时降级到平台原生 API 的统一抽象。
    /// 留作各平台（Windows/Linux/macOS）实现的入口。
    /// </summary>
    public interface IMessageFallback
    {
        bool IsSupported { get; }

        /// <summary>
        /// 弹出原生消息框（阻塞调用线程）。
        /// </summary>
        void ShowMessage(string message, string title, string iconType);

        /// <summary>
        /// 通过平台原生剪贴板 API 写入文本。
        /// </summary>
        bool SetClipboard(string text);
    }
}
