using System;
using System.Collections.Generic;

namespace GGDeals.Settings
{
	public class GGDealsSettings : ObservableObject, IVersionedSettings
	{
		public const int CurrentVersion = 0;

		public GGDealsSettings()
		{
			Version = CurrentVersion;
		}

		public static GGDealsSettings Default =>
			new GGDealsSettings
			{
				LibrariesToSkip = new List<Guid>()
				{
					Guid.Parse("CB91DFC9-B977-43BF-8E70-55F46E410FAB"), // Steam
                    Guid.Parse("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E"), // GOG
                }
			};

		public int Version { get; set; }

		public List<Guid> LibrariesToSkip { get; set; }

		public bool SyncPlayniteLibrary { get; set; }
	}
}