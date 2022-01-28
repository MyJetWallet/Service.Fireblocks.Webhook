using System.Runtime.Serialization;

namespace Service.Fireblocks.Webhook.Grpc.Models.Balances
{
    [DataContract]
    public class UpdateBalancesRequest
    {
        [DataMember(Order = 1)]
        public string AsssetSymbol { get; set; }

        [DataMember(Order = 2)]
        public string AsssetNetwork { get; set; }
    }
}