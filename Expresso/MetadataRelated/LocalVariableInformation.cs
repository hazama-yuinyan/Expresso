using System.Reflection.Metadata;

namespace Expresso.CodeGen
{
    /// <summary>
    /// Holds informations about LocalVariable table.
    /// </summary>
    public struct LocalVariableInformation
    {
        public LocalVariableAttributes Attributes{
            get; set;
        }

        public int Index{
            get; set;
        }

        public string Name{
            get; set;
        }

        public LocalVariableInformation(LocalVariableAttributes attributes, int index, string name)
        {
            Attributes = attributes;
            Index = index;
            Name = name;
        }
    }
}
