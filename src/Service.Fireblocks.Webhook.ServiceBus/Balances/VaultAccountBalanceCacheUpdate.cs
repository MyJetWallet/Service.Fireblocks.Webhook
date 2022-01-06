using Service.Fireblocks.Webhook.Domain.Models.Deposits;
using System.Runtime.Serialization;

namespace Service.Fireblocks.Webhook.ServiceBus.Balances
{
    [DataContract]
    public class VaultAccountBalanceCacheUpdate
    {
        [DataMember(Order = 1)]
        public string[] VaultAccountIds { get; set; }

        [DataMember(Order = 2)]
        public string AssetSymbol { get; set; }

        [DataMember(Order = 3)]
        public string AssetNetwork { get; set; }

        [DataMember(Order = 4)]
        public string FireblocksAssetId { get; set; }
    }
}
