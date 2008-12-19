namespace ObjectMapping.Database.Expression
{
    class Max : Expression
    {
        private string propertyName;

        public Max()
        {
        }

        public Max(string propertyName)
        {
            this.propertyName = propertyName;
        }

        public override string ToSqlString()
        {
            return string.Format("Max ({0})", propertyName); 

        }
    }
    
}
