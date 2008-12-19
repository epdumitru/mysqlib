namespace ObjectMapping.Database.Expression
{
    public class Having
    {
        private Expression condition;
        private Query query;

        public Having()
        {
        }

        public Having(Expression condition)
        {
            this.condition = condition;
        }

        public Expression Condition
        {
            get { return condition; }
            set { condition = value; }
        }

        public Query Query
        {
            get { return query; }
            set { query = value; }
        }

        public string ToSqlString()
        {
            return (" HAVING " + condition.ToSqlString());
        }
    }
}
