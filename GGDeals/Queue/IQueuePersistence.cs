using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GGDeals.Queue
{
	public interface IQueuePersistence
	{
		Task Save(IReadOnlyCollection<Guid> gameIds);

		Task<IReadOnlyCollection<Guid>> Load();
	}
}