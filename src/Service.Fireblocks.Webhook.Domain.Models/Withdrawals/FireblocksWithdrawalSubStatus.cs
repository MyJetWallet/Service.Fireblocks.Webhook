namespace Service.Fireblocks.Webhook.Domain.Models.Withdrawals
{
    public enum FireblocksWithdrawalSubStatus
    {
        None = 0,
        PolicyLimitReached = 1,
        SigningFailed = 2,
    }
}