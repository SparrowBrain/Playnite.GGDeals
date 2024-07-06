using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Playnite.SDK.Models;

namespace GGDeals.Api.Models
{
	public class GameWithLauncher : Game
	{
		[JsonConverter(typeof(StringEnumConverter))]
		[JsonProperty("gg_launcher")]
		public GGLauncher GGLauncher { get; set; }
	}
}