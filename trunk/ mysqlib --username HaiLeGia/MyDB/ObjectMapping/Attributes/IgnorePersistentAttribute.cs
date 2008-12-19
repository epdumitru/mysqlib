using System;

namespace ObjectMapping.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true) ]
    public class IgnorePersistentAttribute : Attribute
    {
    }
}
