using MyJetWallet.Fireblocks.Domain.Models.VaultAssets;
using MyNoSqlServer.Abstractions;

namespace Service.Blockchain.Wallets.MyNoSql.Addresses
{
    public class VaultAssetNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-fireblocks-webhook-vaultasset";
        public static string GeneratePartitionKey(string vaultAccountId) => $"{vaultAccountId}";
        public static string GenerateRowKey(string assetSymbol, string assetNetwork) =>
            $"{assetSymbol}_{assetNetwork}";

        public string VaultAccountId { get; set; }

        public string AssetSymbol { get; set; }

        public string AssetNetwork { get; set; }

        public VaultAsset VaultAsset { get; set; }

        public static VaultAssetNoSql Create(string vaultAccountId, string assetSymbol, string assetNetwork, VaultAsset vaultAsset)
        {
            return new VaultAssetNoSql()
            {
                PartitionKey = GeneratePartitionKey(vaultAccountId),
                RowKey = GenerateRowKey(assetSymbol, assetNetwork),
                VaultAsset = vaultAsset,
                AssetNetwork = assetNetwork,
                AssetSymbol = assetSymbol,
                VaultAccountId = vaultAccountId
            };
        }

    }
}
