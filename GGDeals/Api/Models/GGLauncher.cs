using System.Runtime.Serialization;

namespace GGDeals.Api.Models
{
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