namespace ObjectMapping.Database.Expression
{
    public class Not : Expression
    {
        private Expression expr;

        public Not()
        {
        }

        public Not(Expression expr)
        {
            this.expr = expr;
        }

        public override string ToSqlString()
        {
            return string.Format(string.Format("NOT {0}" , expr));

        }
    }
}
