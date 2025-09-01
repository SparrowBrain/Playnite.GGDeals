using System.ComponentModel;
using System.Runtime.Serialization;

namespace GGDeals.Api.Models
{
	public enum GGLauncher
	{
		[EnumMember(Value = "other")]
		[Description("LOC_GGDeals_GGDealsLauncher_Other")]
		Other = 0,

		[EnumMember(Value = "steam")]
		[Description("LOC_GGDeals_GGDealsLauncher_Steam")]
		Steam = 1,

		[EnumMember(Value = "ea")]
		[Description("LOC_GGDeals_GGDealsLauncher_EA")]
		EA = 2,

		[EnumMember(Value = "ubisoft")]
		[Description("LOC_GGDeals_GGDealsLauncher_Ubisoft")]
		Ubisoft = 3,

		[EnumMember(Value = "gog")]
		[Description("LOC_GGDeals_GGDealsLauncher_Gog")]
		GOG = 4,

		[EnumMember(Value = "epic")]
		[Description("LOC_GGDeals_GGDealsLauncher_Epic")]
		Epic = 5,

		[EnumMember(Value = "microsoft")]
		[Description("LOC_GGDeals_GGDealsLauncher_Microsoft")]
		Microsoft = 6,

		[EnumMember(Value = "battle-net")]
		[Description("LOC_GGDeals_GGDealsLauncher_BattleNet")]
		BattleNet = 7,

		[EnumMember(Value = "rockstar")]
		[Description("LOC_GGDeals_GGDealsLauncher_Rockstar")]
		Rockstar = 8,

		[EnumMember(Value = "prime-gaming")]
		[Description("LOC_GGDeals_GGDealsLauncher_PrimeGaming")]
		PrimeGaming = 9,

		[EnumMember(Value = "playstation")]
		[Description("LOC_GGDeals_GGDealsLauncher_PlayStation")]
		Playstation = 10,

		[EnumMember(Value = "nintendo")]
		[Description("LOC_GGDeals_GGDealsLauncher_Nintendo")]
		Nintendo = 11,

		[EnumMember(Value = "itch")]
		[Description("LOC_GGDeals_GGDealsLauncher_Itch")]
		Itch = 12,

		[EnumMember(Value = "drm-free")]
		[Description("LOC_GGDeals_GGDealsLauncher_DrmFree")]
		DrmFree = 13,
	}
}