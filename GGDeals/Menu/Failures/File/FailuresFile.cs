using GGDeals.Services;
using System;
using System.Collections.Generic;

namespace GGDeals.Menu.Failures.File
{
	public class FailuresFile : IVersionedFailuresFile
	{
		public const int CurrentVersion = 1;

		public FailuresFile()
		{
			Version = CurrentVersion;
		}

		public int Version { get; set; }

		public Dictionary<Guid, AddResult> Failures { get; set; } = new Dictionary<Guid, AddResult>();
	}
}