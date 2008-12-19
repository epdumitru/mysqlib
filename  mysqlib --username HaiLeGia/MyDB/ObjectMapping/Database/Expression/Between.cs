namespace ObjectMapping.Database.Expression
{
    public class Between : Expression
    {
        private string propertyName;
        private object value1;
        private object value2;

        public Between(){}

        public Between(string propertyName, object value1, object value2)
        {
            this.propertyName = propertyName;
            this.value1 = value1;
            this.value2 = value2;
        }

        public override string ToSqlString()
        {
            return string.Format("{0} BETWEEN '{1}' AND '{2}'", propertyName, value1, value2);
        }
    }
}
