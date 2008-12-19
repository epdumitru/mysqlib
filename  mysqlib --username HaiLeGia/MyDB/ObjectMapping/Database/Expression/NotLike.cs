namespace ObjectMapping.Database.Expression
{
    public class NotLike:Expression
    {
        private string propertyName;
        private object value;

        public NotLike()
        {
        }

        public NotLike(string propertyName, object value)
        {
            this.propertyName = propertyName;
            this.value = value;
        }

        public override string ToSqlString()
        {
            return string.Format("({0} NOT LIKE '{1}')", propertyName, value);
        }
    }
}
