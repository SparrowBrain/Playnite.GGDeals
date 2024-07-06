using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GGDeals.Settings.MVVM
{
	public class GGDealsSettingsViewModel : ObservableObject, ISettings
	{
		private readonly ILogger _logger = LogManager.GetLogger();
		private readonly GGDeals _plugin;
		private GGDealsSettings _settings;
		private GGDealsSettings _editingClone;
		private string _authenticationStatus;
		private List<LibraryItem> _libraryItems;

		public GGDealsSettingsViewModel(GGDeals plugin)
		{
			_plugin = plugin;

			var savedSettings = plugin.LoadPluginSettings<GGDealsSettings>();
			Settings = savedSettings ?? GGDealsSettings.Default;
			InitializeLibraryItems();
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

		public List<LibraryItem> LibraryItems
		{
			get => _libraryItems;
			set => SetValue(ref _libraryItems, value);
		}

		private void InitializeLibraryItems()
		{
			var items = new List<LibraryItem>
			{
				new LibraryItem(Guid.Empty, "Playnite", false, Settings.SyncPlayniteLibrary, UpdateSyncPlayniteLibrary)
			};

			foreach (var plugin in _plugin.PlayniteApi.Addons.Plugins.Where(x => x is LibraryPlugin))
			{
				var library = (LibraryPlugin)plugin;
				var isOffByDefault = GGDealsSettings.Default.LibrariesToSkip.Contains(library.Id);
				var isChecked = Settings.LibrariesToSkip.Contains(library.Id) == false;
				var item = new LibraryItem(library.Id, library.Name, isOffByDefault, isChecked, UpdateLibrariesToSkipForLibrary);
				items.Add(item);
			}

			LibraryItems = items;
		}

		private void UpdateLibrariesToSkipForLibrary(Guid id, bool isChecked)
		{
			switch (isChecked)
			{
				case true when Settings.LibrariesToSkip.Contains(id):
					Settings.LibrariesToSkip.Remove(id);
					break;

				case false when !Settings.LibrariesToSkip.Contains(id):
					Settings.LibrariesToSkip.Add(id);
					break;
			}
		}

		private void UpdateSyncPlayniteLibrary(Guid id, bool isChecked)
		{
			Settings.SyncPlayniteLibrary = isChecked;
		}
	}
}