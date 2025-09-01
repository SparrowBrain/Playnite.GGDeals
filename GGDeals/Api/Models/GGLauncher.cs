using System.ComponentModel;
using System.Runtime.Serialization;

namespace GGDeals.Api.Models
{
	public enum GGLauncher
	{
		[EnumMember(Value = "other")]
		[Description("Other")]
		Other = 0,

		[EnumMember(Value = "steam")]
		[Description("Steam")]
		Steam = 1,

		[EnumMember(Value = "ea")]
		[Description("EA App")]
		EA = 2,

		[EnumMember(Value = "ubisoft")]
		[Description("Ubisoft Connect")]
		Ubisoft = 3,

		[EnumMember(Value = "gog")]
		[Description("GOG")]
		GOG = 4,

		[EnumMember(Value = "epic")]
		[Description("Epic Games Launcher")]
		Epic = 5,

		[EnumMember(Value = "microsoft")]
		[Description("Microsoft Store")]
		Microsoft = 6,

		[EnumMember(Value = "battle-net")]
		[Description("Battle.net")]
		BattleNet = 7,

		[EnumMember(Value = "rockstar")]
		[Description("Rockstar Games Launcher")]
		Rockstar = 8,

		[EnumMember(Value = "prime-gaming")]
		[Description("Prime Gaming")]
		PrimeGaming = 9,

		[EnumMember(Value = "playstation")]
		[Description("PlayStation")]
		Playstation = 10,

		[EnumMember(Value = "nintendo")]
		[Description("Nintendo")]
		Nintendo = 11,

		[EnumMember(Value = "itch")]
		[Description("Itch.io")]
		Itch = 12,

		[EnumMember(Value = "drm-free")]
		[Description("DRM free")]
		DrmFree = 13,
	}
}