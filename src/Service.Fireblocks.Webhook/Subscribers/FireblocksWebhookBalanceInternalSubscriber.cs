using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Fireblocks.Client;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using Service.Blockchain.Wallets.MyNoSql.Addresses;
using Service.Fireblocks.Api.Grpc;
using Service.Fireblocks.Webhook.ServiceBus.Balances;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Fireblocks.Webhook.Subscribers
{
    public class FireblocksWebhookBalanceInternalSubscriber
    {
        private readonly ILogger<FireblocksWebhookBalanceInternalSubscriber> _logger;
        private readonly IMyNoSqlServerDataWriter<VaultAssetNoSql> _vaultAssetNoSql;
        private readonly IVaultAccountService _vaultClient;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphoreDict;
        private readonly object _locker = new object();

        public FireblocksWebhookBalanceInternalSubscriber(
            ISubscriber<VaultAccountBalanceCacheUpdate> subscriber,
            ILogger<FireblocksWebhookBalanceInternalSubscriber> logger,
            IMyNoSqlServerDataWriter<VaultAssetNoSql> vaultAssetNoSql,
            IVaultAccountService vaultClient)
        {
            subscriber.Subscribe(HandleSignal);
            _logger = logger;
            _vaultAssetNoSql = vaultAssetNoSql;
            _vaultClient = vaultClient;
            _semaphoreDict = new ConcurrentDictionary<string, SemaphoreSlim>();
        }

        private async ValueTask HandleSignal(VaultAccountBalanceCacheUpdate message)
        {
            using var activity = MyTelemetry.StartActivity("Handle Fireblocks VaultAccountBalanceCacheUpdate");
            var context = message.ToJson();

            _logger.LogInformation("Processing VaultAccountBalanceCacheUpdate: {@context}", context);


            if (message.VaultAccountIds.Any())
            {
                SemaphoreSlim semaphore;
                if (!_semaphoreDict.TryGetValue(message.FireblocksAssetId, out semaphore))
                {
                    lock(_locker)
                    {
                        if (!_semaphoreDict.TryGetValue(message.FireblocksAssetId, out semaphore))
                        {
                            semaphore = new SemaphoreSlim(1);
                            _semaphoreDict.TryAdd(message.FireblocksAssetId, semaphore);
                        }
                    }
                }

                await semaphore.WaitAsync();

                try
                {
                    foreach (var vaultAccountId in message.VaultAccountIds)
                    {
                        var vaultAcc = await _vaultClient.GetVaultAccountAsync(
                            new Api.Grpc.Models.VaultAccounts.GetVaultAccountRequest
                            {
                                VaultAccountId = vaultAccountId
                            });

                        if (vaultAcc.Error == null)
                        {
                            var firstAcc = vaultAcc.VaultAccount.FirstOrDefault();
                            var asset = firstAcc?.VaultAssets
                                .FirstOrDefault(x => x.Id == message.FireblocksAssetId);

                            if (asset != null)
                            {
                                if (asset.Total == 0)
                                {
                                    await _vaultAssetNoSql.DeleteAsync(VaultAssetNoSql.GeneratePartitionKey(vaultAccountId),
                                        VaultAssetNoSql.GenerateRowKey(message.AssetSymbol,
                                    message.AssetNetwork));

                                    _logger.LogInformation("Delete VaultAccountBalanceCacheUpdate: {@context}", context);
                                }
                                else
                                {
                                    await _vaultAssetNoSql.InsertOrReplaceAsync(VaultAssetNoSql.Create(vaultAccountId,
                                    message.AssetSymbol,
                                    message.AssetNetwork,
                                    asset,
                                    firstAcc.Name));

                                    _logger.LogInformation("Processing VaultAccountBalanceCacheUpdate: {context}, {asset}",
                                                            context, asset.ToJson());
                                }
                            }
                            else
                            {
                                _logger.LogError("There is no balance for fireblocks asset {@context}", message);
                            }
                        }
                        else
                        {
                            _logger.LogError("There is no assets for fireblocks vault account {@context}", message);
                        }
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing balance change {@context}", message);
                    ex.FailActivity();
                    throw;
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }
    }
}
