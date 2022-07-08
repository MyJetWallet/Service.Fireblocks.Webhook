using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Fireblocks.Client;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using MyNoSqlServer.Abstractions;
using Service.AssetsDictionary.Grpc;
using Service.Blockchain.Wallets.Grpc;
using Service.Blockchain.Wallets.MyNoSql.Addresses;
using Service.Blockchain.Wallets.MyNoSql.AssetsMappings;
using Service.Fireblocks.Webhook.Domain.Models;
using Service.Fireblocks.Webhook.Domain.Models.Deposits;
using Service.Fireblocks.Webhook.Events;
using Service.Fireblocks.Webhook.ServiceBus;
using Service.Fireblocks.Webhook.ServiceBus.Balances;
using Service.Fireblocks.Webhook.ServiceBus.Deposits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Service.Fireblocks.Webhook.Domain.Models.Withdrawals;
using System.Text.RegularExpressions;

namespace Service.Fireblocks.Webhook.Subscribers
{
    public class FireblocksWebhookInternalSubscriber
    {
        private readonly ILogger<FireblocksWebhookInternalSubscriber> _logger;
        private readonly IWalletService _walletService;
        private readonly IMyNoSqlServerDataReader<AssetMappingNoSql> _assetMappingNoSql;
        private readonly IServiceBusPublisher<FireblocksDepositSignal> _depositPublisher;
        private readonly IServiceBusPublisher<FireblocksDepositWrongAssetSignal> _wrongAssetPublisher;
        private readonly IServiceBusPublisher<FireblocksWithdrawalSignal> _withdrawalPublisher;
        private readonly IServiceBusPublisher<VaultAccountBalanceCacheUpdate> _vaultAccountBalanceCacheUpdatePublisher;
        private readonly Regex _dealerRegex = new Regex(@"dealer[ ]*se[t]{1,2}lement", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private readonly static HashSet<TransactionResponseStatus> _failResponseStatuses = new HashSet<TransactionResponseStatus>
        {
            TransactionResponseStatus.CANCELLED,
            TransactionResponseStatus.FAILED,
            TransactionResponseStatus.BLOCKED,
            TransactionResponseStatus.REJECTED,
            TransactionResponseStatus.TIMEOUT,
        };


        public FireblocksWebhookInternalSubscriber(
            ISubscriber<WebhookQueueItem> subscriber,
            ILogger<FireblocksWebhookInternalSubscriber> logger,
            IWalletService walletService,
            IMyNoSqlServerDataReader<AssetMappingNoSql> assetMappingNoSql,
            IServiceBusPublisher<FireblocksDepositSignal> serviceBusPublisher,
            IServiceBusPublisher<FireblocksDepositWrongAssetSignal> wrongAssetPublisher,
            IServiceBusPublisher<FireblocksWithdrawalSignal> withdrawalPublisher,
            IServiceBusPublisher<VaultAccountBalanceCacheUpdate> vaultAccountBalanceCacheUpdatePublisher)
        {
            subscriber.Subscribe(HandleSignal);
            _logger = logger;
            _walletService = walletService;
            _assetMappingNoSql = assetMappingNoSql;
            _depositPublisher = serviceBusPublisher;
            this._wrongAssetPublisher = wrongAssetPublisher;
            _withdrawalPublisher = withdrawalPublisher;
            _vaultAccountBalanceCacheUpdatePublisher = vaultAccountBalanceCacheUpdatePublisher;
        }

        private async ValueTask HandleSignal(WebhookQueueItem webhook)
        {
            using var activity = MyTelemetry.StartActivity("Handle Fireblocks Event WebhookQueueItem");
            var body = webhook.Data;

            _logger.LogInformation("Processing webhook queue item: {@context}", webhook);

            try
            {
                switch (webhook.Type)
                {
                    case WebhookType.TRANSACTION_CREATED:
                    case WebhookType.TRANSACTION_APPROVAL_STATUS_UPDATED:
                    case WebhookType.TRANSACTION_STATUS_UPDATED:
                        {
                            var transaction = (Newtonsoft.Json.JsonConvert.DeserializeObject<WebhookWithData<TransactionResponse>>(body)).Data;

                            var mapping = _assetMappingNoSql.Get().FirstOrDefault(x => x.AssetMapping.FireblocksAssetId == transaction.AssetId);

                            var feeMapping = _assetMappingNoSql.Get().FirstOrDefault(x => x.AssetMapping.FireblocksAssetId == transaction.FeeCurrency);

                            if (mapping == null)
                            {
                                _logger.LogError("Message from Fireblocks: {@context} ASSET IS NOT CONFIGURED!", body);
                                break;
                            }

                            var assetSymbol = mapping.AssetMapping.AssetId;
                            var feeSymbol = feeMapping?.AssetMapping?.AssetId ?? transaction.FeeCurrency;
                            var network = mapping.AssetMapping.NetworkId;
                            var vaultAccountsList = new List<string>();


                            if (transaction.Status == TransactionResponseStatus.COMPLETED)
                            {
                                if (transaction.Source.Type == TransferPeerPathType.VAULT_ACCOUNT)
                                {
                                    await SendWithdrawalSignalIfPresent(transaction, assetSymbol, network,
                                    FireblocksWithdrawalStatus.Completed, feeSymbol, FireblocksWithdrawalSubStatus.None);
                                    vaultAccountsList.Add(transaction.Source.Id);
                                }

                                bool isDelearManualTransfer = !string.IsNullOrEmpty(transaction.Note) &&
                                        _dealerRegex.IsMatch(transaction.Note);

                                if (isDelearManualTransfer)
                                    _logger.LogInformation("TRANSACTION FROM DEALER! {@context}", webhook);

                                bool isGasTransaction = !string.IsNullOrEmpty(transaction.Source.Name) &&
                                    transaction.Source.Name.ToLowerInvariant().Contains("gas station");

                                if (isGasTransaction)
                                    _logger.LogInformation("TRANSACTION FROM GAS STATION! {@context}", webhook);

                                bool isDeposit = transaction.Destination.Type == TransferPeerPathType.VAULT_ACCOUNT;

                                if (isDeposit)
                                {
                                    vaultAccountsList.Add(transaction.Destination.Id);
                                }

                                if (isDeposit && !isGasTransaction && !isDelearManualTransfer)
                                {
                                    var destTag = transaction.DestinationTag ?? string.Empty;

                                    var users = await _walletService.GetUserByAddressAsync(new Blockchain.Wallets.Grpc.Models.UserWallets.GetUserByAddressRequest
                                    {
                                        Addresses = new Blockchain.Wallets.Grpc.Models.UserWallets.GetUserByAddressRequest.AddressAndTag[]
                                        {
                                            new Blockchain.Wallets.Grpc.Models.UserWallets.GetUserByAddressRequest.AddressAndTag
                                            {
                                                Address = transaction.DestinationAddress.ToLowerInvariant(),
                                                Tag = destTag,
                                            }
                                        },
                                    });

                                    if (users.Error != null)
                                    {
                                        _logger.LogError("Error from Blockchain.Wallets");
                                        throw new Exception("Error from Blockchain.Wallets");
                                    }

                                    if (users.Users != null && users.Users.Any())
                                    {
                                        var firstUser = users.Users.First();

                                        var createdAt = DateTimeOffset.FromUnixTimeMilliseconds((long)transaction.CreatedAt);
                                        if (assetSymbol == firstUser.AssetSymbol)
                                        {
                                            await _depositPublisher.PublishAsync(new FireblocksDepositSignal
                                            {
                                                Amount = transaction.Amount,
                                                AssetSymbol = assetSymbol,
                                                Network = network,
                                                BrokerId = firstUser.BrokerId,
                                                ClientId = firstUser.ClientId,
                                                WalletId = firstUser.WalletId,
                                                Comment = "",
                                                EventDate = createdAt.DateTime,
                                                FeeAmount = transaction.NetworkFee,
                                                FeeAssetSymbol = transaction.FeeCurrency,
                                                Status = FireblocksDepositStatus.Completed,

                                                TransactionId = transaction.TxHash,
                                            });
                                        }
                                        else
                                        {
                                            await _wrongAssetPublisher.PublishAsync(new FireblocksDepositWrongAssetSignal
                                            {
                                                Amount = transaction.Amount,
                                                AssetSymbol = assetSymbol,
                                                Network = network,
                                                BrokerId = firstUser.BrokerId,
                                                ClientId = firstUser.ClientId,
                                                WalletId = firstUser.WalletId,
                                                Comment =
                                                $"It was expected to receive {firstUser.AssetSymbol} {firstUser.AssetNetwork}",
                                                EventDate = createdAt.DateTime,
                                                FeeAmount = transaction.NetworkFee,
                                                FeeAssetSymbol = transaction.FeeCurrency,
                                                Status = FireblocksDepositStatus.Completed,

                                                TransactionId = transaction.TxHash,
                                            });
                                            _logger.LogWarning("Received asset for wrong address! {@context}", webhook);
                                        }
                                    }
                                    else
                                    {
                                        _logger.LogWarning("there is no user for this address! {@context}", webhook);
                                    }
                                }

                                if (transaction.Destinations.Any())
                                {
                                    _logger.LogWarning("Many connections! {@context}", webhook);
                                    //TODO:
                                }

                                if (vaultAccountsList.Any())
                                {
                                    await _vaultAccountBalanceCacheUpdatePublisher.PublishAsync(new VaultAccountBalanceCacheUpdate
                                    {
                                        VaultAccountIds = vaultAccountsList.ToArray(),
                                        AssetSymbol = assetSymbol,
                                        AssetNetwork = network,
                                        FireblocksAssetId = mapping.AssetMapping.FireblocksAssetId,
                                    });
                                }

                            }
                            else if (_failResponseStatuses.Contains(transaction.Status))
                            {
                                _logger.LogWarning(
                                    "Message from Fireblocks Queue: {@context} transaction status indicates failure",
                                    body);

                                if (transaction.Source.Type == TransferPeerPathType.VAULT_ACCOUNT)
                                {

                                    var subStatus = transaction.Status == TransactionResponseStatus.BLOCKED
                                                && transaction.SubStatus == TransactionSubStatus.BLOCKED_BY_POLICY
                                    ? FireblocksWithdrawalSubStatus.PolicyLimitReached
                                    : FireblocksWithdrawalSubStatus.None;

                                    if ((transaction.Status == TransactionResponseStatus.FAILED
                                        && transaction.SubStatus == TransactionSubStatus.SIGNING_ERROR) ||
                                        transaction.Status == TransactionResponseStatus.SIGNING_ERROR)
                                        subStatus = FireblocksWithdrawalSubStatus.SigningFailed;

                                    await SendWithdrawalSignalIfPresent(transaction, assetSymbol, network,
                                        FireblocksWithdrawalStatus.Failed, feeSymbol, subStatus);
                                }
                            }

                            break;
                        }

                    default:
                        {
                            _logger.LogWarning("Message from Fireblocks Queue: {@context} webhook can't be reckognised", body);

                            return;
                        }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook queue {@context}", webhook);
                ex.FailActivity();
                throw;
            }
        }

        private async Task SendWithdrawalSignalIfPresent(TransactionResponse transaction, string assetSymbol,
            string network, FireblocksWithdrawalStatus status, string feeSymbol, FireblocksWithdrawalSubStatus subStatus)
        {
            var createdAt = DateTimeOffset.FromUnixTimeMilliseconds((long)transaction.CreatedAt);
            await _withdrawalPublisher.PublishAsync(new FireblocksWithdrawalSignal()
            {
                Amount = transaction.Amount,
                AssetSymbol = assetSymbol,
                Network = network,
                Comment = $"{transaction.Status}",
                EventDate = createdAt.DateTime,
                FeeAmount = transaction.NetworkFee,
                FeeAssetSymbol = feeSymbol,
                Status = status,
                TransactionId = transaction.TxHash,
                ExternalId = transaction.ExternalTxId,
                InternalNote = transaction.Note,
                DestinationAddress = transaction.DestinationAddress,
                DestinationTag = transaction.DestinationTag,
                SubStatus = subStatus,
            });
        }
    }
}
