using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.Service.Tools;
using MyJetWallet.Sdk.ServiceBus;
using MyNoSqlServer.Abstractions;
using Service.Blockchain.Wallets.MyNoSql.AssetsMappings;
using Service.Fireblocks.Webhook.ServiceBus.Balances;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Fireblocks.Webhook.Jobs
{
    public class BalanceUpdateJob : IStartable, IDisposable
    {
        private readonly MyTaskTimer _timer;
        private readonly ILogger<BalanceUpdateJob> _logger;
        private readonly IServiceBusPublisher<StartBalanceCacheUpdate> _balanceCacheUpdatePublisher;
        private readonly IMyNoSqlServerDataReader<AssetMappingNoSql> _assetMappingNoSql;

        public BalanceUpdateJob(
            ILogger<BalanceUpdateJob> logger,
            IServiceBusPublisher<StartBalanceCacheUpdate> balanceCacheUpdatePublisher,
            IMyNoSqlServerDataReader<AssetMappingNoSql> assetMappingNoSql)
        {
            _timer = new MyTaskTimer(typeof(BalanceUpdateJob), TimeSpan.FromMinutes(Program.Settings.BalanceUpdatePeriodInSec), logger, DoProcess);
            _logger = logger;
            _balanceCacheUpdatePublisher = balanceCacheUpdatePublisher;
            _assetMappingNoSql = assetMappingNoSql;
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        private async Task DoProcess()
        {
            try
            {
                _logger.LogInformation("Starting balance Update");
                var assetMappings = _assetMappingNoSql.Get();

                if (assetMappings == null)
                    return;

                _logger.LogInformation("Starting balance Update");

                foreach (var item in assetMappings)
                {
                    _logger.LogInformation("Requesting balance update for {context}", item.ToJson());
                    await _balanceCacheUpdatePublisher.PublishAsync(new StartBalanceCacheUpdate
                    {
                        AssetNetwork = item.AssetMapping.NetworkId,
                        AssetSymbol = item.AssetMapping.AssetId,
                        FireblocksAssetId = item.AssetMapping.FireblocksAssetId,
                    });
                }

                _logger.LogInformation("Balance Update completed");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "When Updating balances");
                throw;
            }
        }

    }
}
