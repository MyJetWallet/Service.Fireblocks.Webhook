﻿using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Fireblocks.Client;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using MyNoSqlServer.Abstractions;
using Service.Blockchain.Wallets.Grpc;
using Service.Blockchain.Wallets.MyNoSql.AssetsMappings;
using Service.Fireblocks.Webhook.Domain.Models;
using Service.Fireblocks.Webhook.Domain.Models.Deposits;
using Service.Fireblocks.Webhook.Events;
using Service.Fireblocks.Webhook.ServiceBus;
using Service.Fireblocks.Webhook.ServiceBus.Deposits;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Fireblocks.Webhook.Subscribers
{
    public class FireblocksWebhookInternalSubscriber
    {
        private readonly ILogger<FireblocksWebhookInternalSubscriber> _logger;
        private readonly IWalletService _walletService;
        private readonly IMyNoSqlServerDataReader<AssetMappingNoSql> _assetMappingNoSql;
        private readonly IServiceBusPublisher<FireblocksDepositSignal> _depositPublisher;
        private readonly IServiceBusPublisher<FireblocksWithdrawalSignal> _withdrawalPublisher;

        public FireblocksWebhookInternalSubscriber(
            ISubscriber<WebhookQueueItem> subscriber,
            ILogger<FireblocksWebhookInternalSubscriber> logger,
            IWalletService walletService,
            IMyNoSqlServerDataReader<AssetMappingNoSql> assetMappingNoSql,
            IServiceBusPublisher<FireblocksDepositSignal> serviceBusPublisher,
            IServiceBusPublisher<FireblocksWithdrawalSignal> withdrawalPublisher)
        {
            subscriber.Subscribe(HandleSignal);
            this._logger = logger;
            this._walletService = walletService;
            this._assetMappingNoSql = assetMappingNoSql;
            this._depositPublisher = serviceBusPublisher;
            this._withdrawalPublisher = withdrawalPublisher;
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

                            if (mapping == null)
                            {
                                _logger.LogWarning("Message from Fireblocks: {@context} ASSET IS NOT CONFIGURED!", body);
                                break;
                            }

                            var assetSymbol = mapping.AssetMapping.AssetId;
                            var network = mapping.AssetMapping.NetworkId;
                            

                            if (transaction.Status == TransactionResponseStatus.COMPLETED)
                            {
                                if (!string.IsNullOrEmpty(transaction.ExternalTxId))
                                {
                                    var createdAt = DateTimeOffset.FromUnixTimeMilliseconds((long)transaction.CreatedAt);
                                    await _withdrawalPublisher.PublishAsync(new FireblocksWithdrawalSignal()
                                    {
                                        Amount = transaction.Amount,
                                        AssetSymbol = assetSymbol,
                                        Network = network,
                                        Comment = "",
                                        EventDate = createdAt.DateTime,
                                        FeeAmount = transaction.NetworkFee,
                                        FeeAssetSymbol = transaction.FeeCurrency,
                                        Status = Domain.Models.Withdrawals.FireblocksWithdrawalStatus.Completed,
                                        TransactionId = transaction.TxHash,
                                        ExternalId = transaction.ExternalTxId,
                                    });
                                }

                                if (transaction.Destination.Type == TransferPeerPathType.VAULT_ACCOUNT)
                                {
                                    var users = await _walletService.GetUserByAddressAsync(new Blockchain.Wallets.Grpc.Models.UserWallets.GetUserByAddressRequest
                                    {
                                        Addresses = new Blockchain.Wallets.Grpc.Models.UserWallets.GetUserByAddressRequest.AddressAndTag[]
                                        {
                                            new Blockchain.Wallets.Grpc.Models.UserWallets.GetUserByAddressRequest.AddressAndTag
                                            {
                                                Address = transaction.DestinationAddress.ToLowerInvariant(),
                                                Tag = transaction.DestinationTag ?? string.Empty,
                                            }
                                        },
                                    });

                                    if (users.Error != null)
                                    {
                                        _logger.LogError("Error from Blockchain.Wallets");
                                        throw new Exception("Error from Blockchain.Wallets");
                                    }

                                    if (users.Users == null || !users.Users.Any())
                                    {
                                        _logger.LogError("there is no user for this address! {@context}", webhook);
                                        return;
                                    }

                                    var firstUser = users.Users.First();

                                    var createdAt = DateTimeOffset.FromUnixTimeMilliseconds((long)transaction.CreatedAt);
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

                                    break;
                                }

                                if (transaction.Destinations.Any())
                                {
                                    _logger.LogWarning("Many connections! {@context}", webhook);
                                    //TODO:
                                }
                                   
                            } else if (transaction.Status == TransactionResponseStatus.CANCELLED ||
                                       transaction.Status == TransactionResponseStatus.FAILED ||
                                       transaction.Status == TransactionResponseStatus.BLOCKED ||
                                       transaction.Status == TransactionResponseStatus.REJECTED ||
                                       transaction.Status == TransactionResponseStatus.TIMEOUT) {
                                if (!string.IsNullOrEmpty(transaction.ExternalTxId))
                                {
                                    var createdAt = DateTimeOffset.FromUnixTimeMilliseconds((long)transaction.CreatedAt);
                                    await _withdrawalPublisher.PublishAsync(new FireblocksWithdrawalSignal()
                                    {
                                        Amount = transaction.Amount,
                                        AssetSymbol = assetSymbol,
                                        Network = network,
                                        Comment = "",
                                        EventDate = createdAt.DateTime,
                                        FeeAmount = transaction.NetworkFee,
                                        FeeAssetSymbol = transaction.FeeCurrency,
                                        Status = Domain.Models.Withdrawals.FireblocksWithdrawalStatus.Failed,
                                        TransactionId = transaction.TxHash,
                                        ExternalId = transaction.ExternalTxId,
                                    });
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
    }
}