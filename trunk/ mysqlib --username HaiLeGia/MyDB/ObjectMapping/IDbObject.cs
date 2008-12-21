using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectMapping
{
	public interface IDbObject
	{
		long Id { get; set; }
		long UpdateTime { get; set; }
		bool IsDirty { get; set; }
	}
}
