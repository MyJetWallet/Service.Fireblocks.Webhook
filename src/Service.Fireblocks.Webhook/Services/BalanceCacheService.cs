using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using MyNoSqlServer.Abstractions;
using Service.Blockchain.Wallets.MyNoSql.AssetsMappings;
using Service.Fireblocks.Webhook.Grpc;
using Service.Fireblocks.Webhook.Grpc.Models.Balances;
using Service.Fireblocks.Webhook.ServiceBus.Balances;

// ReSharper disable InconsistentLogPropertyNaming
// ReSharper disable TemplateIsNotCompileTimeConstantProblem
// ReSharper disable UnusedMember.Global

namespace Service.Fireblocks.Webhook.Services
{
    public class BalanceCacheService : IBalanceCacheService
    {
        private readonly ILogger<BalanceCacheService> _logger;
        private readonly IServiceBusPublisher<StartBalanceCacheUpdate> _startBalanceMessagePublisher;
        private readonly IMyNoSqlServerDataReader<AssetMappingNoSql> _assetMappingNoSql;

        public BalanceCacheService(ILogger<BalanceCacheService> logger,
            IServiceBusPublisher<StartBalanceCacheUpdate> startBalanceMessagePublisher,
            IMyNoSqlServerDataReader<AssetMappingNoSql> assetMappingNoSql)
        {
            _logger = logger;
            _startBalanceMessagePublisher = startBalanceMessagePublisher;
            _assetMappingNoSql = assetMappingNoSql;
        }

        public async Task<UpdateBalancesResponse> UpdateBalancesAndCacheAsync(UpdateBalancesRequest request)
        {
            try
            {
                var assetMapping = _assetMappingNoSql.Get(
                    AssetMappingNoSql.GeneratePartitionKey(request.AsssetSymbol),
                    AssetMappingNoSql.GenerateRowKey(request.AsssetNetwork));

                if (assetMapping == null)
                    return new UpdateBalancesResponse
                    {
                        Error = new Grpc.Models.Common.ErrorResponse
                        {
                            Message = "Asset mapping does not exist",
                            ErrorCode = Grpc.Models.Common.ErrorCode.DoesNotExist
                        }
                    };

                await _startBalanceMessagePublisher.PublishAsync(new StartBalanceCacheUpdate
                {
                    AssetNetwork = request.AsssetNetwork,
                    AssetSymbol = request.AsssetSymbol,
                    FireblocksAssetId = assetMapping.AssetMapping.FireblocksAssetId,
                });

                return new UpdateBalancesResponse { };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating Transfer {context}", request.ToJson());

                return new UpdateBalancesResponse
                {
                    Error = new Grpc.Models.Common.ErrorResponse
                    {
                        ErrorCode = Grpc.Models.Common.ErrorCode.Unknown,
                        Message = e.Message
                    }
                };
            }
        }
    }
}