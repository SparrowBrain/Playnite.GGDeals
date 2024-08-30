using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace GGDeals.Api.Models
{
    public class GameWithLauncher
    {
        public static GameWithLauncher FromGame(Game game, GGLauncher ggLauncher)
        {
            return new GameWithLauncher
            {
                GGLauncher = ggLauncher,
                Id = game.Id,
                GameId = game.GameId,
                Links = game.Links,
                Source = game.Source,
                ReleaseDate = game.ReleaseDate,
                ReleaseYear = game.ReleaseYear,
                Name = game.Name,
            };
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("gg_launcher")]
        public GGLauncher GGLauncher { get; set; }

        public Guid Id { get; set; }

        public string GameId { get; set; }

        public IReadOnlyCollection<Link> Links { get; set; }

        public GameSource Source { get; set; }

        public ReleaseDate? ReleaseDate { get; set; }

        public int? ReleaseYear { get; set; }

        public string Name { get; set; }
    }
}