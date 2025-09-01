using GGDeals.Api.Models;
using System;
using System.Collections.Generic;

namespace GGDeals.Settings
{
	public class GGDealsSettings : ObservableObject, IVersionedSettings
	{
		public const int CurrentVersion = 1;

		private string _authenticationToken;
		private string _devCollectionImportEndpoint;
		private bool _addLinksToGames;
		private bool _addTagsToGames;
		private bool _syncNewlyAddedGames;
		private bool _showProgressBar;

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
			},
			SyncPlayniteLibrary = false,
			AddLinksToGames = false,
			AddTagsToGames = true,
			SyncNewlyAddedGames = false,
			ShowProgressBar = true,
		};

		public int Version { get; set; }

		public string AuthenticationToken
		{
			get => _authenticationToken;
			set => SetValue(ref _authenticationToken, value);
		}

		public List<Guid> LibrariesToSkip { get; set; }

		public Dictionary<Guid, GGLauncher> LibraryMapOverride { get; set; } = new Dictionary<Guid, GGLauncher>();

		public bool SyncPlayniteLibrary { get; set; }

		public string DevCollectionImportEndpoint
		{
			get => _devCollectionImportEndpoint;
			set => SetValue(ref _devCollectionImportEndpoint, value);
		}

		public bool AddLinksToGames
		{
			get => _addLinksToGames;
			set => SetValue(ref _addLinksToGames, value);
		}

		public bool AddTagsToGames
		{
			get => _addTagsToGames;
			set => SetValue(ref _addTagsToGames, value);
		}

		public bool SyncNewlyAddedGames
		{
			get => _syncNewlyAddedGames;
			set => SetValue(ref _syncNewlyAddedGames, value);
		}

		public bool ShowProgressBar
		{
			get => _showProgressBar;
			set => SetValue(ref _showProgressBar, value);
		}
	}
}