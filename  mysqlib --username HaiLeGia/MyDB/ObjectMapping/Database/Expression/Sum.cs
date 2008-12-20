using System;

namespace ObjectMapping.Database.Expression
{
    public class Sum : Expression
    {
        private string propertyName;
    	private Type type;
        
        public Sum(Type type, string propertyName)
        {
            this.propertyName = propertyName;
        	this.type = type;
        }

    	public Type Type
    	{
    		get { return type; }
    	}

    	public override string ToSqlString()
        {
            return string.Format("SUM ({0})", propertyName); 
        }
    }
}
