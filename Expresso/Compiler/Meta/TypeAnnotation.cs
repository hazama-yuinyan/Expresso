using System;

namespace Expresso.Compiler.Meta
{
	public enum ObjectTypes
	{
		_SUBSCRIPT = -6,
		_CASE_DEFAULT = -5,
		_LABEL_PRIVATE = -4,
		_LABEL_PROTECTED = -3,
		_LABEL_PUBLIC = -2,
		_INFERENCE = -1,
		UNDEF = 0,
		NULL,
		INTEGER,
		BOOL,
		FLOAT,
		RATIONAL,
		BIGINT,
		STRING,
		BYTEARRAY,
		VAR,
		TUPLE,
		LIST,
		DICT,
		EXPRESSION,
		FUNCTION,
		SEQ,
		INSTANCE,
		TYPE_CLASS,
		TYPE_MODULE,
		TYPE_STRUCT,
		TYPE_INTERFACE
	};

	/// <summary>
	/// Contains type information of Expresso's object.
	/// </summary>
	public class TypeAnnotation : ICloneable
	{
		public ObjectTypes ObjType{get; internal set;}
		
		public string TypeName{get; internal set;}
		
		public TypeAnnotation(ObjectTypes type, string name = null)
		{
			ObjType = type;
			TypeName = name;
		}
		
		public TypeAnnotation Clone()
		{
			return new TypeAnnotation(ObjType, TypeName);
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
			return (ObjType == ObjectTypes.TYPE_CLASS) ? string.Format("[class: {0}]", TypeName) :
				(ObjType == ObjectTypes.TYPE_MODULE) ? string.Format("[module: {0}]", TypeName) :
					(ObjType == ObjectTypes.TYPE_STRUCT) ? string.Format("[struct: {0}]", TypeName) :
					(TypeName != null) ? string.Format("{0}", TypeName) : string.Format("{0}", ObjType);
		}
		
		public static readonly TypeAnnotation InferenceType = new TypeAnnotation(ObjectTypes._INFERENCE);
		public static readonly TypeAnnotation VariantType = new TypeAnnotation(ObjectTypes.VAR);
		public static readonly TypeAnnotation VoidType = new TypeAnnotation(ObjectTypes.UNDEF);
		internal static readonly TypeAnnotation Subscription = new TypeAnnotation(ObjectTypes._SUBSCRIPT);
	}
}

