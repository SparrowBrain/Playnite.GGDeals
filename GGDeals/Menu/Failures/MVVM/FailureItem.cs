using System.Collections.Generic;
using GGDeals.Services;
using Playnite.SDK.Models;

namespace GGDeals.Menu.Failures.MVVM
{
    public class FailureItem : ObservableObject
    {
        private readonly ShowAddFailuresViewModel _viewModel;
        private bool _isChecked;

        public FailureItem(ShowAddFailuresViewModel viewModel, Game game, string libraryName, AddToCollectionResult result)
        {
            _viewModel = viewModel;
            Game = game;
            LibraryName = libraryName;
            Result = result;
        }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                SetValue(ref _isChecked, value);
                _viewModel.IsAllChecked = null;
            }
        }

        public Game Game { get; }
        public string LibraryName { get; }
        public AddToCollectionResult Result { get; }
    }
}