using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace GGDeals.Services
{
	public class AddLinkService : IAddLinkService
	{
		private readonly IPlayniteAPI _playniteApi;

		public AddLinkService(IPlayniteAPI playniteApi)
		{
			_playniteApi = playniteApi;
		}

		public void AddLink(Game game, string url)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				throw new ArgumentException("URL cannot be empty!");
			}

			if (game.Links?.Any(x => x.Url.Equals(url, StringComparison.CurrentCultureIgnoreCase)) ?? false)
			{
				return;
			}

			_playniteApi.MainView.UIDispatcher.Invoke(() =>
			{
				if (game.Links == null)
				{
					game.Links = new ObservableCollection<Link>();
				}

				var link = new Link("GG.deals", url);
				game.Links.Add(link);
				_playniteApi.Database.Games.Update(game);
			});
		}
	}
}