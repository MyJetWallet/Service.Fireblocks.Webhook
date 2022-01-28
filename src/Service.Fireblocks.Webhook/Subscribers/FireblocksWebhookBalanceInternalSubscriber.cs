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
using System.Threading.Tasks;

namespace Service.Fireblocks.Webhook.Subscribers
{
    public class FireblocksWebhookBalanceInternalSubscriber
    {
        private readonly ILogger<FireblocksWebhookBalanceInternalSubscriber> _logger;
        private readonly IMyNoSqlServerDataWriter<VaultAssetNoSql> _vaultAssetNoSql;
        private readonly IVaultAccountService _vaultClient;

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
                        var vaultAcc = await _vaultClient.GetVaultAccountAsync(new Api.Grpc.Models.VaultAccounts.GetVaultAccountRequest
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
                                }
                                else
                                {
                                    await _vaultAssetNoSql.InsertOrReplaceAsync(VaultAssetNoSql.Create(vaultAccountId,
                                    message.AssetSymbol,
                                    message.AssetNetwork,
                                    asset,
                                    firstAcc.Name));
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
