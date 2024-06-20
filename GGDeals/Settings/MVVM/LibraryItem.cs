using System;
using System.Collections.Generic;

namespace GGDeals.Settings.MVVM
{
	public class LibraryItem : ObservableObject
	{
		private bool _isChecked;
		private readonly Action<Guid, bool> _isCheckedSettingsUpdateAction;

		public LibraryItem(
			Guid id,
			string name,
			bool isOffByDefault,
			bool isChecked,
			Action<Guid, bool> isCheckedSettingsUpdateAction)
		{
			Id = id;
			Name = name;
			IsOffByDefault = isOffByDefault;
			_isChecked = isChecked;
			_isCheckedSettingsUpdateAction = isCheckedSettingsUpdateAction;
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
	}
}