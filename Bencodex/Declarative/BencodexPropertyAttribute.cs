using System;

namespace Bencodex.Declarative
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BencodexPropertyAttribute : Attribute
    {
        public BencodexPropertyAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
