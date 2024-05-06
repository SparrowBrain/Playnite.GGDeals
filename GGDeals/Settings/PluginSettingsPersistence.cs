namespace GGDeals.Settings
{
    internal class PluginSettingsPersistence : IPluginSettingsPersistence
    {
        private readonly GGDeals _plugin;

        public PluginSettingsPersistence(GGDeals plugin)
        {
            _plugin = plugin;
        }

        public T LoadPluginSettings<T>() where T : class
        {
            return _plugin.LoadPluginSettings<T>();
        }

        public void SavePluginSettings<T>(T settings) where T : class
        {
            _plugin.SavePluginSettings(settings);
        }
    }
}