namespace GGDeals.Settings
{
    public interface IMigratableSettings : IVersionedSettings
    {
        IVersionedSettings Migrate();
    }
}