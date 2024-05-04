using System;
using System.Collections.Generic;

namespace GGDeals.Settings.MVVM
{
    public class LibraryItem : ObservableObject
    {
        private readonly GGDealsSettingsViewModel _viewModel;
        private bool _isChecked;

        public LibraryItem(
            Guid id,
            string name,
            bool isOffByDefault,
            GGDealsSettingsViewModel viewModel,
            bool isChecked)
        {
            Id = id;
            Name = name;
            IsOffByDefault = isOffByDefault;
            _viewModel = viewModel;
            _isChecked = isChecked;
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
                switch (value)
                {
                    case true when _viewModel.Settings.LibrariesToSkip.Contains(Id):
                        _viewModel.Settings.LibrariesToSkip.Remove(Id);
                        break;

                    case false when !_viewModel.Settings.LibrariesToSkip.Contains(Id):
                        _viewModel.Settings.LibrariesToSkip.Add(Id);
                        break;
                }
            }
        }
    }
}