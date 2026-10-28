using Renci.SshNet;
using System;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.Managers
{
    /// <summary>
    /// 基于 SshHostProfile 统一管理 SSH/SFTP 连接的创建、复用与断开
    /// </summary>
    public interface ISshConnectionManager : IDisposable
    {
        /// <summary>
        /// 获取已连接的 Shell 客户端，同一主机复用缓存实例，断线时自动重连
        /// </summary>
        SshClient GetClient(SshHostProfile host);

        /// <summary>
        /// 获取已连接的 SFTP 客户端，同一主机复用缓存实例，断线时自动重连
        /// </summary>
        SftpClient GetSftpClient(SshHostProfile host);

        /// <summary>
        /// 断开并释放指定主机的全部客户端，主机配置修改或删除后应调用
        /// </summary>
        void Disconnect(Guid hostId);

        /// <summary>
        /// 断开并释放所有主机的客户端
        /// </summary>
        void DisconnectAll();
    }
}
