using System;

namespace GGDeals.Api.Models
{
	public class ApiException : Exception
	{
		public ApiException(string message) : base(message)
		{
		}
	}
}