using System;

namespace GGDeals.Api.Models
{
	public class ImportResult
	{
		public Guid Id { get; set; }

		public ImportResultStatus Status { get; set; }

		public string Message { get; set; }
	}
}