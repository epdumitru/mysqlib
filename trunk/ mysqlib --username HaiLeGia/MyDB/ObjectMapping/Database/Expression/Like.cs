namespace ObjectMapping.Database.Expression
{
    public class Like : Expression
    {
        private string propertyName;
        private string value;

        public Like()
        {
        }

        public Like(string propertyName, string value)
        {
            this.propertyName = propertyName;
            this.value = value;
        }

        public override string ToSqlString()
        {
            return string.Format("({0} LIKE '{1}')", propertyName, value);
        }
    }
}
