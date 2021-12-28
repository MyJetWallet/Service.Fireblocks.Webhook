using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Fireblocks.Webhook.Domain.Models
{
    public enum WebhookType
    {
        [System.Runtime.Serialization.EnumMember(Value = @"TRANSACTION_CREATED")]
        TRANSACTION_CREATED,
        [System.Runtime.Serialization.EnumMember(Value = @"TRANSACTION_STATUS_UPDATED")]
        TRANSACTION_STATUS_UPDATED,
        [System.Runtime.Serialization.EnumMember(Value = @"TRANSACTION_APPROVAL_STATUS_UPDATED")]
        TRANSACTION_APPROVAL_STATUS_UPDATED,
        [System.Runtime.Serialization.EnumMember(Value = @"VAULT_ACCOUNT_ADDED")]
        VAULT_ACCOUNT_ADDED,
        [System.Runtime.Serialization.EnumMember(Value = @"VAULT_ACCOUNT_ASSET_ADDED")]
        VAULT_ACCOUNT_ASSET_ADDED,
        [System.Runtime.Serialization.EnumMember(Value = @"INTERNAL_WALLET_ASSET_ADDED")]
        INTERNAL_WALLET_ASSET_ADDED,
        [System.Runtime.Serialization.EnumMember(Value = @"EXTERNAL_WALLET_ASSET_ADDED")]
        EXTERNAL_WALLET_ASSET_ADDED,
        [System.Runtime.Serialization.EnumMember(Value = @"EXCHANGE_ACCOUNT_ADDED")]
        EXCHANGE_ACCOUNT_ADDED,
        [System.Runtime.Serialization.EnumMember(Value = @"FIAT_ACCOUNT_ADDED")]
        FIAT_ACCOUNT_ADDED,
        [System.Runtime.Serialization.EnumMember(Value = @"NETWORK_CONNECTION_ADDED")]
        NETWORK_CONNECTION_ADDED
    }
}
