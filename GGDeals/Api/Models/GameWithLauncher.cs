using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Playnite.SDK.Models;

namespace GGDeals.Api.Models
{
	public class GameWithLauncher : Game
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public GGLauncher GGLauncher { get; set; }
	}
}