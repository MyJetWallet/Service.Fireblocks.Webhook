using Autofac;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.ServiceBus;
using Service.AssetsDictionary.Client;
using Service.Blockchain.Wallets.Client;
using Service.Blockchain.Wallets.MyNoSql.Addresses;
using Service.Blockchain.Wallets.MyNoSql.AssetsMappings;
using Service.Fireblocks.Api.Client;
using Service.Fireblocks.Webhook.Jobs;
using Service.Fireblocks.Webhook.ServiceBus;
using Service.Fireblocks.Webhook.ServiceBus.Balances;
using Service.Fireblocks.Webhook.ServiceBus.Deposits;
using Service.Fireblocks.Webhook.Subscribers;

namespace Service.Fireblocks.Webhook.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));

            builder.RegisterFireblocksApiClient(Program.Settings.FireblocksApiUrl);
            builder.RegisterMyNoSqlReader<AssetMappingNoSql>(myNoSqlClient, AssetMappingNoSql.TableName);
            builder.RegisterMyNoSqlWriter<VaultAssetNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), VaultAssetNoSql.TableName);
            
            builder.RegisterAssetsDictionaryClients(myNoSqlClient);
            builder.RegisterAssetPaymentSettingsClients(myNoSqlClient);
            builder.RegisterBlockchainWalletsClient(Program.Settings.BlockchainWalletsGrpcServiceUrl, myNoSqlClient);

            var serviceBusClient = builder.RegisterMyServiceBusTcpClient(
                Program.ReloadedSettings(e => e.SpotServiceBusHostPort),
                Program.LogFactory);

            builder.RegisterMyServiceBusPublisher<FireblocksDepositSignal>(serviceBusClient, Service.Fireblocks.Webhook.ServiceBus.Topics.FireblocksDepositSignalTopic, false);

            builder.RegisterMyServiceBusPublisher<FireblocksWithdrawalSignal>(serviceBusClient, Service.Fireblocks.Webhook.ServiceBus.Topics.FireblocksWithdrawalSignalTopic, false);

            builder.RegisterMyServiceBusPublisher<WebhookQueueItem>(serviceBusClient, Service.Fireblocks.Webhook.ServiceBus.Topics.FireblocksWebhookInternalTopic, false);

            builder.RegisterMyServiceBusPublisher<StartBalanceCacheUpdate>(serviceBusClient, Service.Fireblocks.Webhook.ServiceBus.Topics.FireblocksWebhookManualBalanceChangeInternalTopic, false);

            builder.RegisterMyServiceBusPublisher<VaultAccountBalanceCacheUpdate>(serviceBusClient, Service.Fireblocks.Webhook.ServiceBus.Topics.FireblocksWebhookBalanceInternalTopic, false);

            builder.RegisterMyServiceBusSubscriberSingle<WebhookQueueItem>(serviceBusClient, 
                Service.Fireblocks.Webhook.ServiceBus.Topics.FireblocksWebhookInternalTopic,
                "service-fireblocks-webhook", MyServiceBus.Abstractions.TopicQueueType.Permanent);

            builder.RegisterMyServiceBusSubscriberSingle<VaultAccountBalanceCacheUpdate>(serviceBusClient,
                Service.Fireblocks.Webhook.ServiceBus.Topics.FireblocksWebhookBalanceInternalTopic,
                "service-fireblocks-webhook", MyServiceBus.Abstractions.TopicQueueType.Permanent);

            builder.RegisterMyServiceBusSubscriberSingle<StartBalanceCacheUpdate>(serviceBusClient,
                Service.Fireblocks.Webhook.ServiceBus.Topics.FireblocksWebhookManualBalanceChangeInternalTopic,
                "service-fireblocks-webhook", MyServiceBus.Abstractions.TopicQueueType.Permanent);

            builder
               .RegisterType<FireblocksWebhookInternalSubscriber>()
               .AutoActivate()
               .SingleInstance();

            builder
               .RegisterType<FireblocksWebhookBalanceInternalSubscriber>()
               .AutoActivate()
               .SingleInstance();

            builder
               .RegisterType<FireblocksWebhookStartBalanceInvalidationInternalSubscriber>()
               .AutoActivate()
               .SingleInstance();

            builder
               .RegisterType<BalanceUpdateJob>()
               .AsSelf()
               .AutoActivate()
               .SingleInstance();
        }
    }
}