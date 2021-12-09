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
    }
}
