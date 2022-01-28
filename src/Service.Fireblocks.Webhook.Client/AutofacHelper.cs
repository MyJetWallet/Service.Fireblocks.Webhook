using Autofac;
using MyJetWallet.Sdk.NoSql;
using MyNoSqlServer.DataReader;
using Service.Blockchain.Wallets.MyNoSql.Addresses;
using Service.Fireblocks.Webhook.Grpc;

namespace Service.Fireblocks.Webhook.Client
{
    public static class AutofacHelper
    {
        public static void RegisterFireblocksWebhookCache(this ContainerBuilder builder, MyNoSqlTcpClient myNoSqlTcpClient)
        {
            builder.RegisterMyNoSqlReader<VaultAssetNoSql>(myNoSqlTcpClient, VaultAssetNoSql.TableName);
        }

        public static void RegisterFireblocksWebhookClient(this ContainerBuilder builder, string grpcUrl)
        {
            var factory = new FireblocksWebhookClientFactory(grpcUrl);
            builder.RegisterInstance<IBalanceCacheService>(factory.GetBalanceCacheServiceService());
        }
    }
}
