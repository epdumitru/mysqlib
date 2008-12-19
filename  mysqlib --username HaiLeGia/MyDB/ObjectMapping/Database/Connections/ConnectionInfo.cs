using System;

namespace ObjectMapping.Database.Connections
{
	public class ConnectionInfo
	{
		private string hostName;
		private string databaseName;
		private string username;
		private string password;
		private string connectionString;
		
		public string HostName
		{
			get { return hostName; }
			set 
			{ 
				hostName = value;
				UpdateConnectionString();
			}
		}
        
		public string DatabaseName
		{
			get { return databaseName; }
			set
			{
				databaseName = value;
				UpdateConnectionString();
			}
		}

		public string Username
		{
			get { return username; }
			set
			{
				username = value;
				UpdateConnectionString();
			}
		}

		public string Password
		{
			get { return password; }
			set
			{
				password = value;
				UpdateConnectionString();
			}
		}

		public string ConnectionString
		{
			get { return connectionString; }
		}

		private void UpdateConnectionString()
		{
			connectionString = String.Format("Data Source = {0}; Database = {1}; username = {2}; password = {3}", hostName, databaseName, username, password);	
		}
	}

}
