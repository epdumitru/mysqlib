namespace ObjectMapping.Database.Expression
{
    class Count : Expression
    {
        private string propertyName;

        public Count()
        {
        }

        public Count(string propertyName)
        {
            this.propertyName = propertyName;
        }

        public override string ToSqlString()
        {
            return string.Format("COUNT ({0})", propertyName); 

        }
    }
}
