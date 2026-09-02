using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.Cli
{
    public class AzureCliBuilder
    {
        public static async Task<string> GetAzureVersionAsync(CancellationToken ct = default)
        {
            string where = await CliClientBuilder.WhereAsync("az");
            ICliClientBuilder builder = CliClientBuilder.Create(where).Args("version --query \\\"azure-cli\\\"");
            CliResult result = await builder.ExecuteAsync(ct);
            return result.Output;
        }
        public static async Task<AzureCliProfile> GetAzureProfileAsync(CancellationToken ct = default)
        {
            string where = await CliClientBuilder.WhereAsync("az");
            ICliClientBuilder builder = CliClientBuilder.Create(where).Args("version");
            CliResult result = await builder.ExecuteAsync(ct);
            return JsonSerializer.Deserialize<AzureCliProfile>(result.Output);
        }
        public static async Task<IAzureCliClientBuilder> CreateAzureAsync(CancellationToken ct = default)
        {
            string where = await CliClientBuilder.WhereAsync("az", ct);

            if (string.IsNullOrEmpty(where)) throw new FileNotFoundException("az");
            FileInfo info = new FileInfo(where);
            if (!info.Exists) throw new FileNotFoundException("az");
            return new AzureCliClientBuilder(info.FullName);
        }
        public static async Task ChangeSubscription(string subscriptionId, CancellationToken ct = default)
        {
            IAzureCliClientBuilder builder = await CreateAzureAsync(ct);
            await builder.Args($"account set --subscription \"{subscriptionId}\"").ExecuteAsync(ct);

        }
    }


}
