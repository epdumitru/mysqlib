namespace ObjectMapping.Database.Expression
{
    public class WherePart 
    {
        private Expression condition;

        public WherePart(Expression condition)
        {
            this.condition = condition;
        }

        public Expression Condition
        {
            get { return condition; }
            set { condition = value; }
        }

        public string ToSqlString()
        {
            return (" WHERE " + condition.ToSqlString());
        }
    }
}
