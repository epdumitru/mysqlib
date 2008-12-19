namespace ObjectMapping.Database.Expression
{
    public class Avg: Expression
    {
        private string propertyName;

        public Avg()
        {
        }

        public Avg(string propertyName)
        {
            this.propertyName = propertyName;
        }

        public override string ToSqlString()
        {
            return string.Format(" AVG ({0})", propertyName);
        }
    }
}
