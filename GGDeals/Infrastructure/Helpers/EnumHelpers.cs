using GGDeals.Api.Models;
using System.ComponentModel;
using System.Reflection;

namespace GGDeals.Infrastructure.Helpers
{
	internal class EnumHelpers
	{
		public static string GetEnumDescription(GGLauncher value)
		{
			var field = typeof(GGLauncher).GetField(value.ToString());
			var attr = field.GetCustomAttribute<DescriptionAttribute>();
			return attr != null ? attr.Description : value.ToString();
		}
	}
}