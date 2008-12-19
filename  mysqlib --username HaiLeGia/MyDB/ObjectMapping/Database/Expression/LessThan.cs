namespace ObjectMapping.Database.Expression
{
    public class LessThan : Expression
    {
        private string propertyName;
        private object value;

        public LessThan()
        {
        }

        public LessThan(string propertyName, object value)
        {
            this.propertyName = propertyName;
            this.value = value;
        }

        public override string ToSqlString()
        {
            return string.Format("({0} < '{1}')", propertyName, value);
        }
    }
}
