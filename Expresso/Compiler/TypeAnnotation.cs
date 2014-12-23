using System;

namespace Expresso.Compiler.Meta
{
    /// <summary>
    /// 実行時の型を表す。
    /// Represents the types at runtime.
    /// </summary>
	public enum ObjectTypes
	{
		_INFERENCE = -1,
		Undef = 0,
		Null,
		Integer,
		Bool,
		Float,
		Rational,
		BigInt,
		String,
        Byte,
		Tuple,
		List,
		Dict,
		Expression,
		Function,
		Seq,
		Instance,
		TypeClass,
		TypeModule,
		TypeStruct,
		TypeInterface
	};

	/// <summary>
	/// Expressoのオブジェクトの型に関するメタデータを保持する。
	/// Contains type information of Expresso's object.
	/// </summary>
	public class TypeAnnotation : ICloneable
	{
		public ObjectTypes ObjType{get; internal set;}
		
		public string TypeName{get; internal set;}

        public bool IsArray{get; internal set;}

        public int Dimension{get; internal set;}
		
		public TypeAnnotation(ObjectTypes type, string name = null)
		{
			ObjType = type;
			TypeName = name;
		}

        public TypeAnnotation(ObjectTypes type, string name, bool isArray, int dimension) : this(type, name)
        {
            IsArray = isArray;
            Dimension = dimension;
        }
		
		public TypeAnnotation Clone()
		{
            return new TypeAnnotation(ObjType, TypeName, IsArray, Dimension);
		}
		
		object ICloneable.Clone()
		{
			return this.Clone();
		}
		
		public override bool Equals(object obj)
		{
			var x = obj as TypeAnnotation;
			if(x == null)
				return false;
			
			return this.ObjType == x.ObjType && this.TypeName == x.TypeName;
		}
		
		public override int GetHashCode()
		{
			return ObjType.GetHashCode() ^ TypeName.GetHashCode();
		}
		
		public override string ToString()
		{
			return (ObjType == ObjectTypes.TypeClass) ? string.Format("[class: {0}]", TypeName) :
				   (ObjType == ObjectTypes.TypeModule) ? string.Format("[module: {0}]", TypeName) :
				   (ObjType == ObjectTypes.TypeStruct) ? string.Format("[struct: {0}]", TypeName) :
				   (TypeName != null) ? string.Format("{0}", TypeName) : string.Format("{0}", ObjType);
		}
		
		public static readonly TypeAnnotation InferenceType = new TypeAnnotation(ObjectTypes._INFERENCE);
		public static readonly TypeAnnotation VariantType = new TypeAnnotation(ObjectTypes.Var);
		public static readonly TypeAnnotation VoidType = new TypeAnnotation(ObjectTypes.Undef);
	}
}

