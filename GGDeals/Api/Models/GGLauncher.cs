using System.ComponentModel;
using System.Runtime.Serialization;

namespace GGDeals.Api.Models
{
	public enum GGLauncher
	{
		[EnumMember(Value = "steam")]
		[Description("LOC_GGDeals_GGDealsLauncher_Steam")]
		Steam,

		[EnumMember(Value = "ea")]
		[Description("LOC_GGDeals_GGDealsLauncher_EA")]
		EA,

		[EnumMember(Value = "ubisoft")]
		[Description("LOC_GGDeals_GGDealsLauncher_Ubisoft")]
		Ubisoft,

		[EnumMember(Value = "gog")]
		[Description("LOC_GGDeals_GGDealsLauncher_Gog")]
		GOG,

		[EnumMember(Value = "epic")]
		[Description("LOC_GGDeals_GGDealsLauncher_Epic")]
		Epic,

		[EnumMember(Value = "microsoft")]
		[Description("LOC_GGDeals_GGDealsLauncher_Microsoft")]
		Microsoft,

		[EnumMember(Value = "battle-net")]
		[Description("LOC_GGDeals_GGDealsLauncher_BattleNet")]
		BattleNet,

		[EnumMember(Value = "rockstar")]
		[Description("LOC_GGDeals_GGDealsLauncher_Rockstar")]
		Rockstar,

		[EnumMember(Value = "prime-gaming")]
		[Description("LOC_GGDeals_GGDealsLauncher_PrimeGaming")]
		PrimeGaming,

		[EnumMember(Value = "playstation")]
		[Description("LOC_GGDeals_GGDealsLauncher_PlayStation")]
		Playstation,

		[EnumMember(Value = "nintendo")]
		[Description("LOC_GGDeals_GGDealsLauncher_Nintendo")]
		Nintendo,

		[EnumMember(Value = "itch")]
		[Description("LOC_GGDeals_GGDealsLauncher_Itch")]
		Itch,

		[EnumMember(Value = "drm-free")]
		[Description("LOC_GGDeals_GGDealsLauncher_DrmFree")]
		DrmFree,

		[EnumMember(Value = "other")]
		[Description("LOC_GGDeals_GGDealsLauncher_Other")]
		Other
	}
}