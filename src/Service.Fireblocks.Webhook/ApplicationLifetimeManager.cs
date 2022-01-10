using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.TcpClient;
using Service.Fireblocks.Webhook.Services;

namespace Service.Fireblocks.Webhook
{
    public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
    {
        private readonly ILogger<ApplicationLifetimeManager> _logger;
        private readonly MyNoSqlClientLifeTime _myNoSqlClient;
        private readonly ServiceBusLifeTime _myServiceBusTcpClient;

        public ApplicationLifetimeManager(
            IHostApplicationLifetime appLifetime,
            ILogger<ApplicationLifetimeManager> logger,
            MyNoSqlClientLifeTime myNoSqlClient,
            ServiceBusLifeTime myServiceBusTcpClient)
            : base(appLifetime)
        {
            _logger = logger;
            this._myNoSqlClient = myNoSqlClient;
            this._myServiceBusTcpClient = myServiceBusTcpClient;
        }

        protected override void OnStarted()
        {
            CryptoProvider.Init();
            _logger.LogInformation("OnStarted has been called");
            _myNoSqlClient.Start();
            _myServiceBusTcpClient.Start();
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called");
            _myNoSqlClient.Stop();
            _myServiceBusTcpClient.Stop();
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called");
        }
    }
}