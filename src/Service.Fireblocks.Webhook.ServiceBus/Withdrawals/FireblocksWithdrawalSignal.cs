using Service.Fireblocks.Webhook.Domain.Models.Deposits;
using Service.Fireblocks.Webhook.Domain.Models.Withdrawals;
using System.Runtime.Serialization;

namespace Service.Fireblocks.Webhook.ServiceBus.Deposits
{
    [DataContract]
    public class FireblocksWithdrawalSignal
    {
        [DataMember(Order = 1)]
        public string ExternalId { get; set; }

        [DataMember(Order = 2)]
        public string TransactionId { get; set; }
        
        [DataMember(Order = 3)]
        public decimal Amount { get; set; }
        
        [DataMember(Order = 4)]
        public string AssetSymbol { get; set; }
        
        [DataMember(Order = 5)]
        public string Comment { get; set; }
        
        [DataMember(Order = 6)]
        public FireblocksWithdrawalStatus Status { get; set; }
        
        [DataMember(Order = 7)]
        public DateTime EventDate { get; set; }
        
        [DataMember(Order = 8)]
        public decimal FeeAmount { get; set; }

        [DataMember(Order = 9)]
        public string FeeAssetSymbol { get; set; }
        
        [DataMember(Order = 10)]
        public string Network { get; set; }
    }
}
