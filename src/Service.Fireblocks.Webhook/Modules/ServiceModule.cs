using Autofac;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using Service.Bitgo.DepositDetector.Client;
using Service.Circle.Signer.Client;
using Service.Circle.Webhooks;

namespace Service.Fireblocks.Webhook.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
        }
    }
}