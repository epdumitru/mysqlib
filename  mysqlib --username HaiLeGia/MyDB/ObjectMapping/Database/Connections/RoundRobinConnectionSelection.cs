﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ObjectMapping.Database.Connections
{
	public class RoundRobinConnectionSelection : IConnectionSelection
	{
		private IList<ConnectionInfo> infors;
		private int index;

	    public IList<ConnectionInfo> Infors
	    {
	        get { return infors; }
	        set { infors = value; }
	    }

	    public ConnectionInfo GetConnectionInfo()
		{
			return infors[Interlocked.Increment(ref index)%infors.Count];
		}
	}
}
