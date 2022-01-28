namespace Service.Fireblocks.Webhook.ServiceBus
{
    public class Topics
    {
        public const string FireblocksDepositSignalTopic = "jet-wallet-fireblocks-deposit-signal";

        public const string FireblocksWithdrawalSignalTopic = "jet-wallet-fireblocks-withdrawal-signal";

        public const string FireblocksWebhookInternalTopic = "jet-wallet-fireblocks-webhook-internal";

        public const string FireblocksWebhookBalanceInternalTopic = "jet-wallet-fireblocks-webhook-balance-internal";

        public const string FireblocksWebhookManualBalanceChangeInternalTopic = "jet-wallet-fireblocks-webhook--manual-balance-internal";
    }
}