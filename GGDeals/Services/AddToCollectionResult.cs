namespace GGDeals.Services
{
    public enum AddToCollectionResult
    {
        Added,
        NotFound,
        Synced,
        SkippedDueToLibrary,
        New,
        Error,
        Ignored
    }
}