using GGDeals.Api.Models;
using GGDeals.Api.Services;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace GGDeals.Settings.MVVM
{
	public class GGDealsSettingsViewModel : ObservableObject, ISettings
	{
		private readonly GGDeals _plugin;
		private readonly ILibraryToGGLauncherMap _libraryToGGLauncherMap;
		private GGDealsSettings _settings;
		private GGDealsSettings _editingClone;
		private List<LibraryItem> _libraryItems;

		public GGDealsSettingsViewModel(GGDeals plugin, ILibraryToGGLauncherMap libraryToGGLauncherMap)
		{
			_plugin = plugin;
			_libraryToGGLauncherMap = libraryToGGLauncherMap;

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

		public ICommand GenerateToken => new RelayCommand(() =>
		{
			Process.Start("https://gg.deals/settings");
		});

		private void InitializeLibraryItems()
		{
			if (!Settings.LibraryMapOverride.TryGetValue(Guid.Empty, out var playniteLauncher))
			{
				playniteLauncher = _libraryToGGLauncherMap.GetGGLauncher(Guid.Empty);
			}
			var items = new List<LibraryItem>
			{
				new LibraryItem(Guid.Empty, "Playnite", false, Settings.SyncPlayniteLibrary, playniteLauncher, UpdateSyncPlayniteLibrary, UpdateLibraryMapOverrideForLibrary)
			};

			foreach (var plugin in _plugin.PlayniteApi.Addons.Plugins.Where(x => x is LibraryPlugin))
			{
				var library = (LibraryPlugin)plugin;
				var isOffByDefault = GGDealsSettings.Default.LibrariesToSkip.Contains(library.Id);
				var isChecked = Settings.LibrariesToSkip.Contains(library.Id) == false;
				if (!Settings.LibraryMapOverride.TryGetValue(library.Id, out var ggLauncher))
				{
					ggLauncher = _libraryToGGLauncherMap.GetGGLauncher(library.Id);
				}

				var item = new LibraryItem(library.Id, library.Name, isOffByDefault, isChecked, ggLauncher, UpdateLibrariesToSkipForLibrary, UpdateLibraryMapOverrideForLibrary);
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

		private void UpdateLibraryMapOverrideForLibrary(Guid pluginId, GGLauncher ggLauncher)
		{
			var defaultLauncher = _libraryToGGLauncherMap.GetGGLauncher(pluginId);
			if (ggLauncher == defaultLauncher)
			{
				Settings.LibraryMapOverride.Remove(pluginId);
			}
			else
			{
				Settings.LibraryMapOverride[pluginId] = ggLauncher;
			}
		}

		private void UpdateSyncPlayniteLibrary(Guid id, bool isChecked)
		{
			Settings.SyncPlayniteLibrary = isChecked;
		}
	}
}