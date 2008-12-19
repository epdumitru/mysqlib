using System;
using ObjectMapping.Attributes;
using ObjectMapping.Database.Expression;

namespace ObjectMapping.Database
{
	public class Query
	{
		protected string sqlQuery;

		public string SqlQuery
		{
			get { return sqlQuery; }
			set { sqlQuery = value; }
		}
	}

	public class Test
	{
		public static void Main(string[] args)
		{
			

		}
	}
}