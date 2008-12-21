using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectMapping
{
	public interface IDirtyObject
	{
		bool IsDirty { get; set; }
	}
}
