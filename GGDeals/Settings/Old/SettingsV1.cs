using System;
using System.Collections.Generic;

namespace GGDeals.Settings.Old
{
	public class SettingsV1 : IVersionedSettings
	{
		public SettingsV1()
		{
			Version = 1;
		}

		public static SettingsV1 Default =>
		new SettingsV1
		{
			LibrariesToSkip = new List<Guid>()
			{
					Guid.Parse("CB91DFC9-B977-43BF-8E70-55F46E410FAB"), // Steam
                    Guid.Parse("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E"), // GOG
			},
			SyncPlayniteLibrary = false,
			AddLinksToGames = false,
			AddTagsToGames = true,
			SyncNewlyAddedGames = false,
			ShowProgressBar = true,
		};

		public int Version { get; set; }

		public string AuthenticationToken { get; set; }

		public List<Guid> LibrariesToSkip { get; set; }

		public bool SyncPlayniteLibrary { get; set; }

		public string DevCollectionImportEndpoint { get; set; }

		public bool AddLinksToGames { get; set; }

		public bool AddTagsToGames { get; set; }

		public bool SyncNewlyAddedGames { get; set; }

		public bool ShowProgressBar { get; set; }
	}
}