using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Fireblocks.Client;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using Service.Blockchain.Wallets.MyNoSql.Addresses;
using Service.Fireblocks.Api.Grpc;
using Service.Fireblocks.Webhook.ServiceBus.Balances;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Fireblocks.Webhook.Subscribers
{
    public class FireblocksWebhookStartBalanceInvalidationInternalSubscriber
    {
        private readonly ILogger<FireblocksWebhookStartBalanceInvalidationInternalSubscriber> _logger;
        private readonly IMyNoSqlServerDataWriter<VaultAssetNoSql> _vaultAssetNoSql;
        private readonly IVaultAccountService _vaultClient;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public FireblocksWebhookStartBalanceInvalidationInternalSubscriber(
            ISubscriber<StartBalanceCacheUpdate> subscriber,
            ILogger<FireblocksWebhookStartBalanceInvalidationInternalSubscriber> logger,
            IMyNoSqlServerDataWriter<VaultAssetNoSql> vaultAssetNoSql,
            IVaultAccountService vaultClient)
        {
            subscriber.Subscribe(HandleSignal);
            _logger = logger;
            _vaultAssetNoSql = vaultAssetNoSql;
            _vaultClient = vaultClient;
        }

        private async ValueTask HandleSignal(StartBalanceCacheUpdate message)
        {
            using var activity = MyTelemetry.StartActivity("Handle Fireblocks StartBalanceCacheUpdate");
            var logContext = message.ToJson();

            _logger.LogInformation("Processing StartBalanceCacheUpdate: {@context}", logContext);

            try
            {
                await _semaphore.WaitAsync();

                foreach (var item in await _vaultAssetNoSql.GetAsync())
                {
                    if (item.AssetNetwork == message.AssetNetwork && item.AssetSymbol == message.AssetSymbol)
                        await _vaultAssetNoSql.DeleteAsync(item.PartitionKey,
                                        item.RowKey);
                }

                var streamBalances = _vaultClient.GetBalancesForAssetAsync(new()
                {
                    BatchSize = 100,
                    FireblocksAssetId = message.FireblocksAssetId,
                    NamePrefix = "",
                    Threshold = 0,
                });

                await foreach (var balances in streamBalances)
                {
                    _logger.LogInformation("Processing stream StartBalanceCacheUpdate: {@context}", new
                    {
                        Balances = balances.ToJson(),
                        Message = logContext,
                    });

                    if (balances.Error != null)
                    {
                        if (balances.Error.ErrorCode == Api.Grpc.Models.Common.ErrorCode.DoesNotExist)
                            break;
                        else
                            throw new Exception(balances.Error.Message);
                    }

                    foreach (var vaultAccount in balances.VaultAccounts)
                    {
                        var vaultAsset = vaultAccount.VaultAssets.FirstOrDefault();

                        if (vaultAsset != null)
                        {
                            if (vaultAsset.Total == 0)
                            {
                                await _vaultAssetNoSql.DeleteAsync(VaultAssetNoSql.GeneratePartitionKey(vaultAccount.Id),
                                    VaultAssetNoSql.GenerateRowKey(message.AssetSymbol,
                                message.AssetNetwork));
                            }
                            else
                            {
                                await _vaultAssetNoSql.InsertOrReplaceAsync(VaultAssetNoSql.Create(vaultAccount.Id,
                                message.AssetSymbol,
                                message.AssetNetwork,
                                vaultAsset,
                                vaultAccount.Name));
                            }
                        }
                        else
                        {
                            _logger.LogError("There is no balance for fireblocks asset {@context}", message);
                        }
                    }
                }

                _logger.LogInformation("Completed StartBalanceCacheUpdate: {@context}", logContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing StartBalanceCacheUpdate {@context}", message);
                ex.FailActivity();
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
