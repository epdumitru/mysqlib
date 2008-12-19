using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectMapping.Database.Connections
{
	public interface IConnectionSelection
	{
		ConnectionInfo GetConnectionInfo();
	}
}
