namespace GGDeals.Services
{
    public enum AddToCollectionResult
    {
        Added,
        PageNotFound,
        AlreadyOwned,
        SkippedDueToLibrary,
        NotProcessed,
        Error
    }
}