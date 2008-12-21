using System.Collections.Generic;
using System.IO;

namespace ObjectMapping
{
	public interface ICacheable
	{
		long Id { get; set; }
		object SyncRootObject { get; set; }
	}
}
