using System;
using System.Collections.Generic;
using Playnite.SDK.Models;

namespace GGDeals.Services
{
	public interface IAddResultProcessor
	{
		void Process(IReadOnlyCollection<Game> games, IDictionary<Guid, AddResult> results);
	}
}