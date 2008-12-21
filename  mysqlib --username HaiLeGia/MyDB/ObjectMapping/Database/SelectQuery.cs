using System;
using System.Collections.Generic;
using System.Text;
using ObjectMapping.Attributes;
using ObjectMapping.Database.Expression;

namespace ObjectMapping.Database
{
	public class SelectQuery : Query
	{
		public static string[] ALL_PROPS = new string[0];
		protected WherePart wherePart;
		protected Having having;
		protected List<OrderBy> orderBy;
		protected int top;
		protected int limit;
		protected Type type;
		protected string mappingTable;
		protected IList<string> propertyNames;

		public SelectQuery(Type type, int top, int limit, params string[] propertyNames)
		{
			this.type = type;
			var classMetadatas = type.GetCustomAttributes(typeof(PersistentAttribute), true);
			mappingTable = ((PersistentAttribute)classMetadatas[0]).MappingTable;
			this.top = top;
			this.limit = limit;
			this.type = type;
			if (propertyNames != ALL_PROPS) 
			{
				var tempProperties = new List<string>(propertyNames);
				if (Array.IndexOf(propertyNames, "Id") < 0)
				{
					tempProperties.Add("Id");	
				}
				if (Array.IndexOf(propertyNames, "UpdateTime") < 0)
				{
					tempProperties.Add("UpdateTime");
				}
				this.propertyNames = tempProperties;
			}
			else
			{
				this.propertyNames = propertyNames;	
			}
			
		}

		public virtual SelectQuery Where(Expression.Expression condition)
		{
			wherePart = new WherePart(condition);
			return this;
		}

		public virtual SelectQuery OrderBy(OrderBy orderBy)
		{
			this.orderBy.Add(orderBy);
			return this;
		}

		public virtual SelectQuery Having(Having having)
		{
			this.having = having;
			return this;
		}

		public virtual IList<string> PropertyNames
		{
			get { return propertyNames; }
		}

		public virtual string ToSqlString()
		{
			var str = new StringBuilder("SELECT ");//"* FROM " + mappingTable + " ");
			if (propertyNames == ALL_PROPS)
			{
				str.Append("* ");	
			}
			else
			{
				for (var i = 0; i < propertyNames.Count; i++)
				{
					str.Append(propertyNames[i] + ", ");
				}
				var tmpString = str.ToString(0, str.Length - 2);
				str.Length = 0;
				str.Append(tmpString);
			}
			str.Append(" FROM " + mappingTable + " ");
			if (wherePart != null)
			{
				str.Append(wherePart.ToSqlString());
			}
			if (having != null)
			{
				str.Append(having.ToSqlString());
			}
			if (orderBy != null && orderBy.Count > 0)
			{
				int n = orderBy.Count;
				str.Append(" ORDER BY " + orderBy[0].ToSqlString());
				for (int i = 1; i < n; i++)
				{
					str.Append(string.Format(", {0}", orderBy[i].ToSqlString()));
				}
			}
			str.Append(string.Format(" TOP {0} Limit {1}", top, limit));
			sqlQuery = str.ToString();
			return sqlQuery;
		}	
	}

}
