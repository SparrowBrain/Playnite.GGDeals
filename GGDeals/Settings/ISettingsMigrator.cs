namespace GGDeals.Settings
{
    public interface ISettingsMigrator
    {
        GGDealsSettings LoadAndMigrateToNewest(int version);
    }
}