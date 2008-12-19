namespace ObjectMapping.Database.Expression
{
    class Min: Expression
    {
        private string propertyName;

        public Min()
        {
        }

        public Min(string propertyName)
        {
            this.propertyName = propertyName;
        }

        public override string ToSqlString()
        {
            return string.Format("Min ({0})", propertyName); 

        }
    }
}
