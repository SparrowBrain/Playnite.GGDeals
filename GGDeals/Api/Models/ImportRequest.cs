using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK.Models;

namespace GGDeals.Api.Models
{
	public class ImportRequest
	{
		public string Version => "v1";

		public string Token { get; set; }

		public string Data { get; set; }
	}

	public class ImportResponse
	{
		public bool Success { get; set; }

		public ResponseData Data { get; set; }
	}

	public class ResponseData
	{
		public List<ImportResult> Result { get; set; }
	}

	public class ImportResult
	{
		public Guid Id { get; set; }

		public ImportResultStatus Status { get; set; }

		public string Message { get; set; }
	}

	public enum ImportResultStatus
	{
		Error,
		Added,
		Skipped,
		Miss,
	}

	public class GameWithLauncher : Game
	{
		public GGLauncher GGLauncher { get; set; }
	}

	public enum GGLauncher
	{
		[EnumMember(Value = "steam")]
		Steam,

		[EnumMember(Value = "ea")]
		EA,

		[EnumMember(Value = "ubisoft")]
		Ubisoft,

		[EnumMember(Value = "gog")]
		GOG,

		[EnumMember(Value = "epic")]
		Epic,

		[EnumMember(Value = "microsoft")]
		Microsoft,

		[EnumMember(Value = "battle-net")]
		BattleNet,

		[EnumMember(Value = "rockstar")]
		Rockstar,

		[EnumMember(Value = "prime-gaming")]
		PrimeGaming,

		[EnumMember(Value = "playstation")]
		Playstation,

		[EnumMember(Value = "nintendo")]
		Nintendo,

		[EnumMember(Value = "itch")]
		Itch,

		[EnumMember(Value = "drm-free")]
		DrmFree,

		[EnumMember(Value = "other")]
		Other
	}
}