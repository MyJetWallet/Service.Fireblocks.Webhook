using Service.Fireblocks.Webhook.Domain.Models;

namespace Service.Fireblocks.Webhook.Events
{
    public class WebhookBase
    {
        [Newtonsoft.Json.JsonProperty("type", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public WebhookType Type { get; set; }

        [Newtonsoft.Json.JsonProperty("tenantId", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string TenantId { get; set; }

        [Newtonsoft.Json.JsonProperty("timestamp", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public long Timestamp { get; set; }
    }

    public class WebhookWithData<T> : WebhookBase
    {
        public T Data { get; set; }
    }
}
