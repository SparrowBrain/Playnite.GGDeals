using System.Collections.Generic;
using Playnite.SDK;
using Playnite.SDK.Data;

namespace GGDeals.Settings
{
    public class GGDealsSettingsViewModel : ObservableObject, ISettings
    {
        private readonly GgDeals _plugin;
        private GGDealsSettings _settings;
        private GGDealsSettings _editingClone;

        public GGDealsSettingsViewModel(GgDeals plugin)
        {
            _plugin = plugin;

            var savedSettings = plugin.LoadPluginSettings<GGDealsSettings>();
            Settings = savedSettings ?? new GGDealsSettings();
        }

        public GGDealsSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public void BeginEdit()
        {
            _editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            Settings = _editingClone;
        }

        public void EndEdit()
        {
            _plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }
    }
}