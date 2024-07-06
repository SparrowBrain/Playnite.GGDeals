namespace GGDeals.Services
{
    public enum AddToCollectionResult
    {
        Added,
        Missed,
        AlreadyOwned,
        SkippedDueToLibrary,
        NotProcessed,
        Error
    }
}