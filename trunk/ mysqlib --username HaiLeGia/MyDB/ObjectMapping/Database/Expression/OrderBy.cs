namespace ObjectMapping.Database.Expression
{
    public class OrderBy 
    {
        public const string ASC = "ASC";
        public const string DESC = "DESC";
        private string propertyname;
        private string orderBy;

        public OrderBy()
        {
        }

        public OrderBy(string propertyname)
        {
            this.propertyname = propertyname;
            orderBy = ASC;
        }

        public OrderBy(string propertyname, string orderBy)
        {
            this.propertyname = propertyname;
            this.orderBy = orderBy;
        }

        public string ToSqlString()
        {
            return string.Format(" {0} {1}", propertyname, orderBy);
        }
    }
}
