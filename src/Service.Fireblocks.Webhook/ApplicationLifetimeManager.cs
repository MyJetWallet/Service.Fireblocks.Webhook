using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.TcpClient;
using Service.Fireblocks.Webhook.Jobs;
using Service.Fireblocks.Webhook.Services;

namespace Service.Fireblocks.Webhook
{
    public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
    {
        private readonly ILogger<ApplicationLifetimeManager> _logger;
        private readonly MyNoSqlClientLifeTime _myNoSqlClient;
        private readonly ServiceBusLifeTime _myServiceBusTcpClient;
        private readonly BalanceUpdateJob _balanceUpdateJob;

        public ApplicationLifetimeManager(
            IHostApplicationLifetime appLifetime,
            ILogger<ApplicationLifetimeManager> logger,
            MyNoSqlClientLifeTime myNoSqlClient,
            ServiceBusLifeTime myServiceBusTcpClient,
            BalanceUpdateJob balanceUpdateJob)
            : base(appLifetime)
        {
            _logger = logger;
            _myNoSqlClient = myNoSqlClient;
            _myServiceBusTcpClient = myServiceBusTcpClient;
            _balanceUpdateJob = balanceUpdateJob;
        }

        protected override void OnStarted()
        {
            CryptoProvider.Init();
            _logger.LogInformation("OnStarted has been called");
            _myNoSqlClient.Start();
            _myServiceBusTcpClient.Start();
            _balanceUpdateJob.Start();
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called");
            _myNoSqlClient.Stop();
            _myServiceBusTcpClient.Stop();
            _balanceUpdateJob.Dispose();
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called");
        }
    }
}