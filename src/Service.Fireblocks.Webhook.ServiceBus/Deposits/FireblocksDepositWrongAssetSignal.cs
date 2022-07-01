using Service.Fireblocks.Webhook.Domain.Models.Deposits;
using System.Runtime.Serialization;

namespace Service.Fireblocks.Webhook.ServiceBus.Deposits
{
    [DataContract]
    public class FireblocksDepositWrongAssetSignal
    {
        public const string ServiceBusMessageTopic = "jet-wallet-fireblocks-deposit-wrong-asset-signal";

        [DataMember(Order = 1)]
        public string BrokerId { get; set; }

        [DataMember(Order = 2)]
        public string ClientId { get; set; }

        [DataMember(Order = 3)]
        public string WalletId { get; set; }

        [DataMember(Order = 4)]
        public string TransactionId { get; set; }

        [DataMember(Order = 5)]
        public decimal Amount { get; set; }

        [DataMember(Order = 6)]
        public string AssetSymbol { get; set; }

        [DataMember(Order = 7)]
        public string Comment { get; set; }

        [DataMember(Order = 8)]
        public FireblocksDepositStatus Status { get; set; }

        [DataMember(Order = 9)]
        public DateTime EventDate { get; set; }

        [DataMember(Order = 10)]
        public decimal FeeAmount { get; set; }

        [DataMember(Order = 11)]
        public string FeeAssetSymbol { get; set; }

        [DataMember(Order = 12)]
        public string Network { get; set; }
    }
}
