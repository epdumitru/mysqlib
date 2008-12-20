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
		protected string[] propertyNames;

		public SelectQuery(Type type, int top, int limit, params string[] propertyNames)
		{
			this.type = type;
			var classMetadatas = type.GetCustomAttributes(typeof(PersistentAttribute), false);
			mappingTable = ((PersistentAttribute)classMetadatas[0]).MappingTable;
			this.top = top;
			this.limit = limit;
			this.type = type;
			this.propertyNames = propertyNames;
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

		public virtual string[] PropertyNames
		{
			get { return propertyNames; }
		}

		public virtual string ToSqlString()
		{
			StringBuilder str = new StringBuilder("SELECT * FROM " + mappingTable + " ");
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
