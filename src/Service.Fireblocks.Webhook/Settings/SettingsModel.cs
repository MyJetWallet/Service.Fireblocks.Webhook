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
        public string MyNoSqlReaderHostPort { get; internal set; }

        [YamlProperty("FireblocksWebhook.BlockchainWalletsGrpcServiceUrl")]
        public string BlockchainWalletsGrpcServiceUrl { get; internal set; }

        [YamlProperty("FireblocksWebhook.SpotServiceBusHostPort")]
        public string SpotServiceBusHostPort { get; internal set; }

        [YamlProperty("FireblocksWebhook.MyNoSqlWriterUrl")]
        public string MyNoSqlWriterUrl { get; internal set; }
    }
}
