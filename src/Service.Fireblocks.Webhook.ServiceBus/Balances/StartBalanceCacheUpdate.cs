using System.Runtime.Serialization;

namespace Service.Fireblocks.Webhook.ServiceBus.Balances
{
    [DataContract]
    public class StartBalanceCacheUpdate
    {
        [DataMember(Order = 1)]
        public string AssetSymbol { get; set; }

        [DataMember(Order = 2)]
        public string AssetNetwork { get; set; }

        [DataMember(Order = 3)]
        public string FireblocksAssetId { get; set; }
    }
}
