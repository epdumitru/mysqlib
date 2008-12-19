using System;

namespace ObjectMapping.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class PersistentAttribute : Attribute
    {
    	private string mappingTable;

    	public string MappingTable
    	{
    		get { return mappingTable; }
    		set { mappingTable = value; }
    	}
    }
}
