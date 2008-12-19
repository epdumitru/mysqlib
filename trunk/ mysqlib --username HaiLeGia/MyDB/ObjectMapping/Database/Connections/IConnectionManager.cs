using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace ObjectMapping.Database.Connections
{
	public interface IConnectionManager
	{
		DbConnection GetReadConnection();
		DbConnection GetUpdateConnection();
	}
}
