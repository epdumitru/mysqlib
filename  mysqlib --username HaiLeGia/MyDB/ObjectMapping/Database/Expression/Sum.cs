namespace ObjectMapping.Database.Expression
{
    class Sum: Expression
    {
        private string propertyName;

        public Sum()
        {
        }

        public Sum(string propertyName)
        {
            this.propertyName = propertyName;
        }

        public override string ToSqlString()
        {
            return string.Format("SUM ({0})", propertyName); 

        }
    }
}
