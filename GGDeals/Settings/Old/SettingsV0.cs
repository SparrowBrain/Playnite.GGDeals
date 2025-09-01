using System;
using System.Collections.Generic;

namespace GGDeals.Settings.Old
{
    public class SettingsV0 : IMigratableSettings
	{
        public SettingsV0()
        {
            Version = 0;
        }

        public static SettingsV0 Default =>
            new SettingsV0
            {
                LibrariesToSkip = new List<Guid>()
                {
                    Guid.Parse("CB91DFC9-B977-43BF-8E70-55F46E410FAB"), // Steam
                    Guid.Parse("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E"), // GOG
                },
                SyncPlayniteLibrary = false,
                AddLinksToGames = false,
            };

        public int Version { get; set; }

        public virtual IVersionedSettings Migrate()
        {
            var newSettings = SettingsV1.Default;
            newSettings.AddLinksToGames = AddLinksToGames;
            newSettings.AuthenticationToken = AuthenticationToken;
            newSettings.DevCollectionImportEndpoint = DevCollectionImportEndpoint;
            newSettings.LibrariesToSkip = LibrariesToSkip;
            newSettings.SyncPlayniteLibrary = SyncPlayniteLibrary;

            return newSettings;
        }

        public string AuthenticationToken { get; set; }

        public List<Guid> LibrariesToSkip { get; set; }

        public bool SyncPlayniteLibrary { get; set; }

        public string DevCollectionImportEndpoint { get; set; }

        public bool AddLinksToGames { get; set; }
    }
}