using Service.Fireblocks.Webhook.Grpc.Models.Common;
using System.Runtime.Serialization;

namespace Service.Fireblocks.Webhook.Grpc.Models.Balances
{
    [DataContract]
    public class UpdateBalancesResponse
    {
        [DataMember(Order = 1)]
        public ErrorResponse Error { get; set; }

    }
}