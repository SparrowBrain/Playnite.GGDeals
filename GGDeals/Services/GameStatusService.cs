using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GGDeals.Services
{
	public class GameStatusService : IGameStatusService
	{
		private readonly IPlayniteAPI _playniteApi;
		private readonly IReadOnlyCollection<Tag> _ggDealsTags;

		private readonly Dictionary<string, AddToCollectionResult> _tagToStatusMap =
			new Dictionary<string, AddToCollectionResult>()
			{
				{ "[GGDeals] Synced", AddToCollectionResult.Synced },
				{ "[GGDeals] Ignored", AddToCollectionResult.Ignored },
				{ "[GGDeals] NotFound", AddToCollectionResult.NotFound },
			};

		private readonly Dictionary<AddToCollectionResult, string> _statusToTagMap = new Dictionary<AddToCollectionResult, string>()
		{
			{ AddToCollectionResult.Added, "[GGDeals] Synced" },
			{ AddToCollectionResult.Synced, "[GGDeals] Synced" },
			{ AddToCollectionResult.Ignored, "[GGDeals] Ignored" },
			{ AddToCollectionResult.NotFound, "[GGDeals] NotFound" },
		};

		public GameStatusService(IPlayniteAPI playniteApi)
		{
			_playniteApi = playniteApi;
			_ggDealsTags = playniteApi.Database.Tags.Where(t => t.Name.StartsWith("[GGDeals]")).ToList();
		}

		public AddToCollectionResult GetStatus(Game game)
		{
			Tag gameTag = null;
			foreach (var ggDealsTag in _ggDealsTags)
			{
				if (game.TagIds?.Contains(ggDealsTag.Id) ?? false)
				{
					gameTag = ggDealsTag;
				}
			}

			if (gameTag == null)
			{
				return AddToCollectionResult.New;
			}

			if (!_tagToStatusMap.TryGetValue(gameTag.Name, out var status))
			{
				throw new Exception($"Unknown GGDeals tag: {gameTag.Name}");
			}

			return status;
		}

		public void UpdateStatus(Game game, AddToCollectionResult status)
		{
			var tagName = _statusToTagMap[status];
			var tag = _ggDealsTags.FirstOrDefault(t => t.Name == tagName);
			if (tag == null)
			{
				tag = new Tag { Id = Guid.NewGuid(), Name = tagName };
				_playniteApi.Database.Tags.Add(tag);
			}

			foreach (var ggDealsTag in _ggDealsTags)
			{
				if ((game.TagIds?.Contains(ggDealsTag.Id) ?? false) && ggDealsTag.Id != tag.Id)
				{
					game.TagIds.Remove(ggDealsTag.Id);
				}
			}

			if (!(game.TagIds?.Contains(tag.Id) ?? false))
			{
				if (game.TagIds == null)
				{
					game.TagIds = new List<Guid>();
				}

				game.TagIds.Add(tag.Id);
			}
		}
	}
}