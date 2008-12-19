using System;

namespace ObjectMapping.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class StringAttribute: Attribute
    {
        public const int UNLIMITED_STRING = 1;
        public const int LIMIT_STRING = 2;
        private int numberOfChar = 1024;

        public StringAttribute(int type)
        {
            this.Type = type;
        }

        public StringAttribute(int type, int numberOfChar)
        {
            this.Type = type;
            this.numberOfChar = numberOfChar;
        }

        public int NumberOfChar
        {
            get { return numberOfChar; }
            set { numberOfChar = value; }
        }

        public int Type { get; set; }
    }
}
