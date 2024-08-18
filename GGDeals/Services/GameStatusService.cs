using GGDeals.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GGDeals.Services
{
    public class GameStatusService : IGameStatusService
    {
        private readonly IPlayniteAPI _playniteApi;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

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
        }

        public AddToCollectionResult GetStatus(Game game)
        {
            Tag gameTag = null;
            var ggDealsTags = _playniteApi.Database.Tags.Where(t => t.Name.StartsWith("[GGDeals]")).ToList();
            foreach (var ggDealsTag in ggDealsTags)
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
            var tag = EnsureTagExists(status);
            RemoveOtherGGDealTags(game, tag);
            AddTag(game, tag);

            _playniteApi.Database.Games.Update(game);
        }

        private Tag EnsureTagExists(AddToCollectionResult status)
        {
            var tagName = _statusToTagMap[status];
            var tag = GetTagFromDatabase(tagName);
            if (tag == null)
            {
                _semaphore.Wait();
                tag = GetTagFromDatabase(tagName);
                if (tag == null)
                {
                    tag = new Tag { Id = Guid.NewGuid(), Name = tagName };
                    _playniteApi.Database.Tags.Add(tag);
                }
            }

            return tag;
        }

        private void RemoveOtherGGDealTags(Game game, Tag desiredTag)
        {
            var ggDealsTags = _playniteApi.Database.Tags.Where(t => t.Name.StartsWith("[GGDeals]")).ToList();
            foreach (var ggDealsTag in ggDealsTags)
            {
                if ((game.TagIds?.Contains(ggDealsTag.Id) ?? false) && ggDealsTag.Id != desiredTag.Id)
                {
                    game.TagIds.Remove(ggDealsTag.Id);
                }
            }
        }

        private static void AddTag(Game game, Tag tag)
        {
            if (!(game.TagIds?.Contains(tag.Id) ?? false))
            {
                if (game.TagIds == null)
                {
                    game.TagIds = new List<Guid>();
                }

                game.TagIds.Add(tag.Id);
            }
        }

        private Tag GetTagFromDatabase(string tagName)
        {
            var ggDealsTags = _playniteApi.Database.Tags.Where(t => t.Name.StartsWith("[GGDeals]")).ToList();
            var tag = ggDealsTags.FirstOrDefault(t => t.Name == tagName);
            return tag;
        }
    }
}