using Autofac;
using MyJetWallet.Sdk.NoSql;
using MyNoSqlServer.DataReader;
using Service.Blockchain.Wallets.MyNoSql.Addresses;

namespace Service.Fireblocks.Webhook.Client
{
    public static class AutofacHelper
    {
        public static void RegisterFireblocksWebhookCache(this ContainerBuilder builder, MyNoSqlTcpClient myNoSqlTcpClient)
        {
            builder.RegisterMyNoSqlReader<VaultAssetNoSql>(myNoSqlTcpClient, VaultAssetNoSql.TableName);
        }
    }
}
