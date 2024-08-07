using GGDeals.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GGDeals.Models;

namespace GGDeals.Menu.Failures.File
{
	public interface IAddFailuresFileService
	{
		Task<Dictionary<Guid, AddResult>> Load();

		Task Save(Dictionary<Guid, AddResult> failures);
	}
}