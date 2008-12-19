namespace ObjectMapping.Database.Expression
{
	public class Eq : Expression
	{
		private string propertyName;
		private object value;

		public Eq()
		{
		}

		public Eq(string propertyName, object value)
		{
			this.propertyName = propertyName;
			this.value = value;
		}

		public override string ToSqlString()
		{
			return string.Format("{0} = '{1}'", propertyName, value);
		}
	}
}
