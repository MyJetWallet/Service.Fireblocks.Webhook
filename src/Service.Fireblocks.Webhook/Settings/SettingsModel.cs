using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Fireblocks.Webhook.Settings
{
    public class SettingsModel
    {
        [YamlProperty("FireblocksWebhook.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("FireblocksWebhook.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("FireblocksWebhook.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }

        [YamlProperty("FireblocksWebhook.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }

        [YamlProperty("FireblocksWebhook.BlockchainWalletsGrpcServiceUrl")]
        public string BlockchainWalletsGrpcServiceUrl { get; set; }

        [YamlProperty("FireblocksWebhook.SpotServiceBusHostPort")]
        public string SpotServiceBusHostPort { get; set; }

        [YamlProperty("FireblocksWebhook.MyNoSqlWriterUrl")]
        public string MyNoSqlWriterUrl { get;  set; }

        [YamlProperty("FireblocksWebhook.FireblocksApiUrl")]
        public string FireblocksApiUrl { get;  set; }

        [YamlProperty("FireblocksWebhook.BalanceUpdatePeriodInSec")]
        public int BalanceUpdatePeriodInSec { get;  set; }
    }
}
