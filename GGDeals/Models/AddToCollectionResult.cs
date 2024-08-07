namespace GGDeals.Models
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