using System.Collections.Generic;
using System.Text;

namespace ObjectMapping.Database.Expression
{
	public class Or : Expression
	{
		private List<Expression> exprs;

		public Or()
		{
			exprs = new List<Expression>();
		}

		public Or(params Expression[] exprs)
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
				formatString.Append(" OR ({" + i + "})");
				param[i] = exprs[i].ToSqlString();
			}
			return string.Format(formatString.ToString(), param);
		}

	}
}
