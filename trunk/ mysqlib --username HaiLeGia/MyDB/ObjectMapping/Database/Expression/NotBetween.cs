namespace ObjectMapping.Database.Expression
{
    public class NotBetween : Expression
    {
        private string propertyName;
        private object value1;
        private object value2;

        public NotBetween()
        {
        }

        public NotBetween(string propertyName, object value1, object value2)
        {
            this.propertyName = propertyName;
            this.value1 = value1;
            this.value2 = value2;
        }

        public override string ToSqlString()
        {
            return string.Format("{0} NOT BETWEEN '{1}' AND '{2}'", propertyName, value1, value2);
        }
    }
}
