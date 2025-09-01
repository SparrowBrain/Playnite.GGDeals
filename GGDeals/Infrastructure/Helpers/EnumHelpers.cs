using System.ComponentModel;
using System.Reflection;
using GGDeals.Api.Models;
using Playnite.SDK;

namespace GGDeals.Infrastructure.Helpers
{
	internal class EnumHelpers
	{
		public static string GetEnumDescription(GGLauncher value)
		{
			var field = typeof(GGLauncher).GetField(value.ToString());
			var attr = field.GetCustomAttribute<DescriptionAttribute>();
			return attr != null ? ResourceProvider.GetString(attr.Description) : value.ToString();
		}
	}
}