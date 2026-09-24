using AzureTools.SDK;
using AzureTools.SDK.KeyVaults;
using AzureTools.SDK.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Toolkit.Modules.Azure.KeyVaults
{
    public class KeyVaultCompareService : IKeyVaultCompareService
    {
        public KeyVaultCompareService(ITenantProvider tenantProvider, IAzureResourceFactory keyVaultFactory, ILogger<KeyVaultCompareService> logger)
        {
            _logger = logger;
            _tenantProvider = tenantProvider;
            _keyVaultFactory = keyVaultFactory;
        }

        private readonly ILogger<KeyVaultCompareService> _logger;
        private readonly ITenantProvider _tenantProvider;
        private readonly IAzureResourceFactory _keyVaultFactory;

        public async Task<List<SecretCompareReport>> CompareAsync(params KeyVaultAzureEnv[] options)
        {
            _logger.LogInformation("开始对比，共 {Count} 个环境", options.Length);
            if (options == null || options.Length == 0) return new List<SecretCompareReport>();
            try
            {
                var keyVaults = options.Select(opt => new
                {
                    Option = opt,
                    Management = _keyVaultFactory.GetKeyVaultManagement(opt, _tenantProvider.GetTenant(opt.TenantId))
                }).ToList();

                List<List<SecretItem>> keysResults = new List<List<SecretItem>>();
                foreach (var kv in keyVaults)
                {
                    List<SecretItem> keys = await kv.Management.GetKeys();
                    keysResults.Add(keys);
                }

                List<string> allKeyNames = keysResults.SelectMany(k => k.Select(x => x.Name))
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();

                _logger.LogInformation("对比密钥列表获取完成，共 {Count} 个不同的密钥", allKeyNames.Count);

                var valueTasks = allKeyNames.Select(keyName =>
                {
                    List<bool> keyExists = keysResults.Select(keys => keys.Any(k => k.Name == keyName)).ToList();
                    List<Task<string>> getValueTasks = keyVaults.Zip(keyExists, (kv, exists) =>
                        exists ? kv.Management.Get(keyName) : Task.FromResult<string>(null)).ToList();
                    return Task.WhenAll(getValueTasks).ContinueWith(t => new
                    {
                        keyName,
                        ExistsList = keyExists,
                        Values = t.Result
                    });
                });

                var valueResults = await Task.WhenAll(valueTasks);

                List<SecretCompareReport> results = valueResults.Select(r =>
                {
                    SecretCompareReport result = new SecretCompareReport
                    {
                        KeyName = r.keyName
                    };

                    for (int i = 0; i < options.Length; i++)
                    {
                        string envName = options[i].Name;
                        bool exists = r.ExistsList[i];
                        result.EnvStatusMap[envName] = exists ? "存在" : "缺失";
                        result.EnvValueMap[envName] = r.Values[i];
                    }

                    result.Status = r.ExistsList.All(e => e) ? CompareStatus.AllExist : CompareStatus.Partial;
                    return result;
                }).ToList();

                _logger.LogInformation("对比完成，共 {Count} 条结果", results.Count);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "对比失败，环境：{Envs}", string.Join(", ", options.Select(o => o.Name)));
                throw;
            }
        }
    }
}
