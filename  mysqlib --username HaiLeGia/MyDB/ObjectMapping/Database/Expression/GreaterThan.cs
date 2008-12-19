namespace ObjectMapping.Database.Expression
{
    public class GreaterThan : Expression
    {
        private string propertyName;
        private object value;

        public GreaterThan(){}

        public GreaterThan(string propertyName, object value)
        {
            this.propertyName = propertyName;
            this.value = value;
        }

        public override string ToSqlString()
        {
            return string.Format("({0} > '{1}')", propertyName, value);
        }
    }
}
