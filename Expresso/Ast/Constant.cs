using System;
using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Compiler.Meta;

namespace Expresso.Ast
{
    /// <summary>
    /// 定数。
	/// Represents a constant.
    /// </summary>
    public class Constant : Expression
    {
		readonly ObjectTypes val_type;
		readonly object val;

		/// <summary>
		/// この定数値の型。
		/// The type of this constant.
		/// </summary>
		public ObjectTypes ValType{
			get{return val_type;}
		}
		
        /// <summary>
        /// 定数の値。
		/// The value.
        /// </summary>
        public object Value{
			get{return val;}
		}

        public override NodeType Type{
            get{return NodeType.Constant;}
        }

		public Constant(object value, ObjectTypes valType)
		{
			this.val_type = valType;
			this.val = value;
		}

        public override bool Equals(object obj)
        {
            var x = obj as Constant;

            if(x == null)
                return false;

            return val.Equals(x.val) && val_type.Equals(x.val_type);
        }

        public override int GetHashCode()
        {
            return this.val.GetHashCode() ^ val_type.GetHashCode();
        }

        public override void AcceptWalker(AstWalker walker)
		{
			if(walker.Walk(this)){}
			walker.PostWalk(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.Walk(this);
        }

        public override string GetText()
		{
			return string.Format("{0}", val);
		}

        public override string ToString()
        {
            return GetText();
        }
    }
}
