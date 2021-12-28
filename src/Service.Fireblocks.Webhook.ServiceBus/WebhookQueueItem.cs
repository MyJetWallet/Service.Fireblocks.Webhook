using Service.Fireblocks.Webhook.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Service.Fireblocks.Webhook.ServiceBus
{
    [DataContract]
    public class WebhookQueueItem
    {
        [DataMember(Order = 1)]
        public string Data { get; set; }

        [DataMember(Order = 2)]
        public WebhookType Type { get; set; }
    }
}
