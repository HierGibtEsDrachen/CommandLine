using System;

namespace CommandLine.Initializer
{
    public class ContentPropertyAttribute : Attribute
    {
        public string Name { get; }
        public string MethodName { get; }
        public ContentPropertyAttribute(string propertyname, string delegatename)
        {
            Name = propertyname;
            MethodName = delegatename;
        }
        public ContentPropertyAttribute(string propertyname)
        {
            Name = propertyname;
        }
    }
}