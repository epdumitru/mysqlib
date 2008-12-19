using System.Collections.Generic;
using System.Data.Common;
using MySql.Data.MySqlClient;
using ObjectMapping.Database.Connections;

namespace ObjectMapping.MySql.Connections
{
	public class MySqlConnectionManager : IConnectionManager
	{
		private IList<ConnectionInfo> masterInfos;
		private IList<ConnectionInfo> slaveInfos;
		private IConnectionSelection masterConnectionSelection;
		private IConnectionSelection slaveConnectionSelection;

		public IConnectionSelection MasterConnectionSelection
		{
			get { return masterConnectionSelection; }
			set { masterConnectionSelection = value; }
		}

		public IConnectionSelection SlaveConnectionSelection
		{
			get { return slaveConnectionSelection; }
			set { slaveConnectionSelection = value; }
		}

		public IList<ConnectionInfo> SlaveInfos
		{
			get { return slaveInfos; }
			set { slaveInfos = value; }
		}

		public IList<ConnectionInfo> MasterInfos
		{
			get { return masterInfos; }
			set { masterInfos = value; }
		}

		public DbConnection GetReadConnection()
		{
			var info = slaveConnectionSelection.GetConnectionInfo();
			var connection =
				new MySqlConnection(info.ConnectionString);
			connection.Open();
			return connection;
		}

		public DbConnection GetUpdateConnection()
		{
			var info = masterConnectionSelection.GetConnectionInfo();
			var connection =
				new MySqlConnection(info.ConnectionString);
			connection.Open();
			return connection;
		}
	}
}