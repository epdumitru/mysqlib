namespace ObjectMapping.Database.Expression
{
    public class LessThanOrEqual : Expression
    {
        private string propertyName;
        private object value;

        public LessThanOrEqual()
        {
        }

        public LessThanOrEqual(string propertyName, object value)
        {
            this.propertyName = propertyName;
            this.value = value;
        }


        public override string ToSqlString()
        {
            return string.Format("({0} <= '{1}')", propertyName, value);
        }
    }
}
