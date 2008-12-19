using System.Collections.Generic;
using System.Text;

namespace ObjectMapping.Database.Expression
{
	public class And : Expression
	{
		private List<Expression> exprs;

		public And()
		{
			exprs = new List<Expression>();
		}

		public And(params Expression[] exprs)
		{
			this.exprs = new List<Expression>();
			this.exprs.AddRange(exprs);
		}

		public void AddExpr(Expression expr)
		{
			exprs.Add(expr);	
		}

		public override string ToSqlString()
		{
			var formatString = new StringBuilder("({0})");
			var param = new string[exprs.Count];
			param[0] = exprs[0].ToSqlString();
			for (var i = 1; i < exprs.Count; i++)
			{
				formatString.Append(" AND ({" + i + "})");
				param[i] = exprs[i].ToSqlString();
			}
			return string.Format(formatString.ToString(), param);
		}
	}
}
