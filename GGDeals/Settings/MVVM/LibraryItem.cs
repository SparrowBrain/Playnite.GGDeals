using GGDeals.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using GGDeals.Infrastructure.Helpers;

namespace GGDeals.Settings.MVVM
{
	public class LibraryItem : ObservableObject
	{
		private readonly Action<Guid, bool> _isCheckedSettingsUpdateAction;
		private readonly Action<Guid, GGLauncher> _ggLauncherSettingsUpdateAction;
		private bool _isChecked;
		private GGLauncher _ggLauncher;

		public LibraryItem(Guid id,
			string name,
			bool isOffByDefault,
			bool isChecked,
			GGLauncher ggLauncher,
			Action<Guid, bool> isCheckedSettingsUpdateAction,
			Action<Guid, GGLauncher> ggLauncherSettingsUpdateAction)
		{
			Id = id;
			Name = name;
			IsOffByDefault = isOffByDefault;
			_isChecked = isChecked;
			GGLauncher = ggLauncher;
			_isCheckedSettingsUpdateAction = isCheckedSettingsUpdateAction;
			_ggLauncherSettingsUpdateAction = ggLauncherSettingsUpdateAction;
		}

		public Guid Id { get; }

		public string Name { get; }

		public bool IsOffByDefault { get; }

		// ReSharper disable once UnusedMember.Global
		public bool IsChecked
		{
			get => _isChecked;
			set
			{
				SetValue(ref _isChecked, value);
				_isCheckedSettingsUpdateAction(Id, value);
			}
		}

		public GGLauncher GGLauncher
		{
			get => _ggLauncher;
			set
			{
				SetValue(ref _ggLauncher, value);
				_ggLauncherSettingsUpdateAction?.Invoke(Id, value);
			}
		}

		public Dictionary<GGLauncher, string> GGLauncherOptions { get; } =
			Enum.GetValues(typeof(GGLauncher))
				.Cast<GGLauncher>()
				.ToDictionary(
					x => x,
					EnumHelpers.GetEnumDescription
				);
	}
}