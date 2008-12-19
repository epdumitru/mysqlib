using System;
using System.Collections.Generic;
using System.Text;
using ObjectMapping.Attributes;
using ObjectMapping.Database.Expression;

namespace ObjectMapping.Database
{
    public class SelectQuery<T> : Query
    {
        protected WherePart wherePart;
        protected Having having;
        protected List<OrderBy> orderBy;
        protected int top;
        protected int limit;
        protected Type type;
        protected string mappingTable;

        public SelectQuery(int top, int limit)
        {
            this.type = typeof(T);
            var classMetadatas = type.GetCustomAttributes(typeof(PersistentAttribute), false);
            mappingTable = ((PersistentAttribute)classMetadatas[0]).MappingTable;
            this.top = top;
            this.limit = limit;
            this.type = type;
        }

        public SelectQuery<T> Where(Expression.Expression condition)
        {
            wherePart = new WherePart(condition);
            return this;
        }

		public SelectQuery<T> OrderBy(OrderBy orderBy)
		{
			this.orderBy.Add(orderBy);
			return this;
		}

		public SelectQuery<T> Having(Having having)
		{
			this.having = having;
			return this;
		}

		public string ToSqlString()
        {
            StringBuilder str = new StringBuilder("SELECT * FROM " + mappingTable + " ");
            if(wherePart != null)
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
                for (int i = 1; i < n; i++ )
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
