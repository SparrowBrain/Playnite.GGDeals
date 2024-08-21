using System;
using System.Collections.Generic;
using System.Linq;

namespace GGDeals.Models
{
	public class SyncRunSettings
	{
		public List<AddToCollectionResult> StatusesToSync { get; set; } = new List<AddToCollectionResult>();

		public static SyncRunSettings Default { get; } = new SyncRunSettings
		{
			StatusesToSync = new List<AddToCollectionResult>
			{
				AddToCollectionResult.New,
				AddToCollectionResult.NotFound
			}
		};

		public static SyncRunSettings All { get; } = new SyncRunSettings
		{
			StatusesToSync = Enum.GetValues(typeof(AddToCollectionResult)).Cast<AddToCollectionResult>().ToList()
		};
	}
}