using GGDeals.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GGDeals.Menu.AddGames.MVVM
{
	public class AddGamesViewModel : ObservableObject, IViewModelForWindow
	{
		private readonly GGDeals _plugin;
		private readonly Guid? _duplicateHiderTagId;
		private Window _window;
		private bool _addNew;
		private bool _addSynced;
		private bool _addNotFound;
		private bool _addIgnored;

		public AddGamesViewModel(GGDeals plugin)
		{
			_plugin = plugin;
			_duplicateHiderTagId = plugin.PlayniteApi.Database.Tags?.FirstOrDefault(t => t.Name == "[DH] Hidden")?.Id;

			AddNew = true;
		}

		public void AssociateWindow(Window window)
		{
			_window = window;
		}

		public bool AddNew
		{
			get => _addNew;
			set => SetValue(ref _addNew, value);
		}

		public bool AddSynced
		{
			get => _addSynced;
			set => SetValue(ref _addSynced, value);
		}

		public bool AddNotFound
		{
			get => _addNotFound;
			set => SetValue(ref _addNotFound, value);
		}

		public bool AddIgnored
		{
			get => _addIgnored;
			set => SetValue(ref _addIgnored, value);
		}

		// ReSharper disable once UnusedMember.Global
		public ICommand AddAllGames => new RelayCommand(() =>
		{
			var games = _plugin.PlayniteApi.Database.Games.Where(game =>
				!game.Hidden || game.Hidden && _duplicateHiderTagId != null &&
				(game.TagIds?.Contains(_duplicateHiderTagId.Value) ?? false)).ToList();

			var syncRunSettings = new SyncRunSettings
			{
				StatusesToSync = new List<AddToCollectionResult>()
			};

			if (AddNew)
			{
				syncRunSettings.StatusesToSync.Add(AddToCollectionResult.New);
			}

			if (AddSynced)
			{
				syncRunSettings.StatusesToSync.Add(AddToCollectionResult.Synced);
			}

			if (AddNotFound)
			{
				syncRunSettings.StatusesToSync.Add(AddToCollectionResult.NotFound);
			}

			if (AddIgnored)
			{
				syncRunSettings.StatusesToSync.Add(AddToCollectionResult.Ignored);
			}

			_plugin.AddGamesToGGCollection(games, syncRunSettings);
			OnAddingGamesInitiated();
		});

		protected virtual void OnAddingGamesInitiated()
		{
			_window.Close();
		}
	}
}