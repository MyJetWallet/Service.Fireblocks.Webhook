using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.Fireblocks.Webhook.Grpc;

namespace Service.Fireblocks.Webhook.Client
{
    [UsedImplicitly]
    public class FireblocksWebhookClientFactory : MyGrpcClientFactory
    {
        public FireblocksWebhookClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IBalanceCacheService GetBalanceCacheServiceService() => CreateGrpcService<IBalanceCacheService>();
    }
}
