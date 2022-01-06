using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Fireblocks.Client;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using Service.Blockchain.Wallets.MyNoSql.Addresses;
using Service.Fireblocks.Webhook.ServiceBus.Balances;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Fireblocks.Webhook.Subscribers
{
    public class FireblocksWebhookBalanceInternalSubscriber
    {
        private readonly ILogger<FireblocksWebhookBalanceInternalSubscriber> _logger;
        private readonly IMyNoSqlServerDataWriter<VaultAssetNoSql> _vaultAssetNoSql;
        private readonly IVaultClient _vaultClient;

        public FireblocksWebhookBalanceInternalSubscriber(
            ISubscriber<VaultAccountBalanceCacheUpdate> subscriber,
            ILogger<FireblocksWebhookBalanceInternalSubscriber> logger,
            IMyNoSqlServerDataWriter<VaultAssetNoSql> vaultAssetNoSql,
            IVaultClient vaultClient)
        {
            subscriber.Subscribe(HandleSignal);
            _logger = logger;
            _vaultAssetNoSql = vaultAssetNoSql;
            _vaultClient = vaultClient;
        }

        private async ValueTask HandleSignal(VaultAccountBalanceCacheUpdate message)
        {
            using var activity = MyTelemetry.StartActivity("Handle Fireblocks VaultAccountBalanceCacheUpdate");

            _logger.LogInformation("Processing VaultAccountBalanceCacheUpdate: {@context}", message);

            try
            {
                if (message.VaultAccountIds.Any())
                {
                    foreach (var vaultAccountId in message.VaultAccountIds)
                    {
                        var vaultAcc = await _vaultClient.AccountsGetAsync(vaultAccountId, default);

                        if (vaultAcc?.Result?.Assets != null)
                        {
                            var asset = vaultAcc.Result.Assets.FirstOrDefault(x => x.Id == message.FireblocksAssetId);

                            if (asset != null)
                            {
                                await _vaultAssetNoSql.InsertOrReplaceAsync(VaultAssetNoSql.Create(vaultAccountId, 
                                    message.AssetSymbol,
                                    message.AssetNetwork,
                                    new MyJetWallet.Fireblocks.Domain.Models.VaultAssets.VaultAsset
                                    {
                                        Available = decimal.Parse(asset.Available),
                                        BlockHash = asset.BlockHash,
                                        BlockHeight = asset.BlockHeight,
                                        Frozen = decimal.Parse(asset.Frozen),
                                        Id = asset.Id,
                                        LockedAmount = decimal.Parse(asset.LockedAmount),
                                        Pending = decimal.Parse(asset.Pending),
                                        Staked = decimal.Parse(asset.Staked),
                                        Total = decimal.Parse(asset.Total),
                                    }));
                            } else
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing balance change {@context}", message);
                ex.FailActivity();
                throw;
            }
        }
    }
}
