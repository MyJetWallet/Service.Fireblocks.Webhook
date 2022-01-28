using System.ServiceModel;
using System.Threading.Tasks;
using Service.Fireblocks.Webhook.Grpc.Models.Balances;

namespace Service.Fireblocks.Webhook.Grpc
{
    [ServiceContract]
    public interface IBalanceCacheService
    {
        [OperationContract]
        Task<UpdateBalancesResponse> UpdateBalancesAndCacheAsync(UpdateBalancesRequest request);
    }
}