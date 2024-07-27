namespace GGDeals.Services
{
    public enum AddToCollectionResult
    {
        Added,
        Missed,
        AlreadyOwned,
        SkippedDueToLibrary,
        New,
        Error,
        Ignored
    }
}