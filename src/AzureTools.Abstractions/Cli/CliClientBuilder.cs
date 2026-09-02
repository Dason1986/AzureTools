using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.Cli
{
    public class CliClientBuilder
    {
        private static readonly ConcurrentDictionary<string, string> _whereCache = new();

        #region 静态工厂入口

        public static async Task<bool> ExistAsync(string file, CancellationToken ct = default)
        {
            string path = await WhereAsync(file, ct);
            return !string.IsNullOrEmpty(path) && path.Contains(file);
        }
        public static async Task<string> WhereAsync(string file, CancellationToken ct = default)
        {
            if (_whereCache.TryGetValue(file, out string cached))
                return cached;

            string name = OperatingSystem.IsWindows() ? "where" : "which";
            ICliClientBuilder builder = Create(name).Args(file);
            CliResult result = await builder.ExecuteAsync(ct);
            if (!result.Success || !result.Output.Contains(file))
                return string.Empty;

            _whereCache[file] = result.Output;
            return result.Output;
        }


        public static ICliClientBuilder Create(string fileName)
        {
            return new DefaultCliClientBuilder(fileName);
        }
        #endregion
    }


}
