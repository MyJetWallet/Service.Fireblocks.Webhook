using System;
using System.Threading.Tasks;
using ProtoBuf.Grpc.Client;
using Service.Fireblocks.Webhook.Client;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;

            Console.Write("Press enter to start");
            Console.ReadLine();

            var factory = new FireblocksWebhookClientFactory("http://localhost:89");
            var balance = factory.GetBalanceCacheServiceService();
            await balance.UpdateBalancesAndCacheAsync(new Service.Fireblocks.Webhook.Grpc.Models.Balances.UpdateBalancesRequest()
            {
                AsssetNetwork = "fireblocks-eth-test",
                AsssetSymbol = "ETH"
            });

            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}