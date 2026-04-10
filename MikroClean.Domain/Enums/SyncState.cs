namespace MikroClean.Domain.Enums
{
    public enum SyncState
    {
        Synced = 0,
        PendingCreate = 1,
        PendingUpdate = 2,
        PendingDelete = 3,
        SyncFailed = 4
    }
}
