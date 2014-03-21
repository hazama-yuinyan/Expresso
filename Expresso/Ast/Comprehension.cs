using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Builtins;
using Expresso.Runtime;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Compiler.Meta;
using Expresso.Runtime.Operations;


namespace Expresso.Ast
{
	/// <summary>
	/// Represents a list comprehension, which is syntactic sugar for sequence initialization.
    /// Consider an expression, [x for x in [0..100]],
    /// which is equivalent in functionality to the statement "for(let x in [0..100]) yield x;"
	/// </summary>
	public class Comprehension : Expression
	{
		ObjectTypes type;

		public Expression Item{
            get{return (Expression)FirstChild;}
		}

		public ComprehensionFor Body{
            get{return (ComprehensionFor)LastChild;}
		}

		public ObjectTypes ObjType{
			get{return type;}
		}

        public override NodeType Type{
            get{return NodeType.Comprehension;}
        }

		public Comprehension(Expression itemExpr, ComprehensionFor bodyExpr, ObjectTypes objType)
		{
			type = objType;
            AddChild(itemExpr);
            AddChild(bodyExpr);
		}

        public override bool Equals(object obj)
        {
            var x = obj as Comprehension;

            if(x == null)
                return false;

            return Body.Equals(x.Body) && Item.Equals(x.Item);
        }

        public override int GetHashCode()
        {
            return Body.GetHashCode() ^ Item.GetHashCode();
        }

        public override void AcceptWalker(AstWalker walker)
		{
			if(walker.Walk(this)){
                Item.AcceptWalker(walker);
                Body.AcceptWalker(walker);
			}
			walker.PostWalk(this);
		}
	}

	public abstract class ComprehensionIter : Expression
	{
		public abstract IEnumerable<Identifier> LocalVariables{get;}
		//internal override object Run(VariableStore varStore){return null;}
		//internal abstract IEnumerable<object> Run(VariableStore varStore, Expression yieldExpr);
	}

	public class ComprehensionFor : ComprehensionIter
	{
		/// <summary>
        /// body内で操作対象となるオブジェクトを参照するのに使用する式。
        /// 評価結果はlvalueにならなければならない。
        /// なお、走査の対象を捕捉する際には普通の代入と同じルールが適用される。
        /// つまり、複数の変数にいっせいにオブジェクトを捕捉させることもできる。
        /// When evaluating the both sides of the "in" keyword,
        /// the same rule as the assignment applies.
        /// So for example,
        /// for(let x, y in [1,2,3,4,5,6])...
        /// the x and y captures the first and second element of the list at the first time,
        /// the third and forth the next time, and the fifth and sixth at last.
        /// </summary>
        public SequenceExpression Left{
            get{return (SequenceExpression)FirstChild;}
		}

        /// <summary>
        /// 操作する対象の式。
        /// The target expression.
        /// </summary>
        public Expression Target{
            get{return (Expression)FirstChild.NextSibling;}
		}

		/// <summary>
        /// 実行対象の文。
        /// The body that will be executed.
        /// </summary>
		public ComprehensionIter Body{
            get{return (ComprehensionIter)LastChild;}
		}

        public override NodeType Type{
            get{return NodeType.ComprehensionFor;}
        }

		public ComprehensionFor(SequenceExpression lhs, Expression targetExpr, ComprehensionIter bodyExpr)
		{
            AddChild(lhs);
            AddChild(targetExpr);
            AddChild(bodyExpr);
		}

        public override IEnumerable<Identifier> LocalVariables{
			get{
				/*var inner = (Body == null) ? Enumerable.Empty<Identifier>() : Body.LocalVariables;
				var on_this =
					from p in LValues
					select (Identifier)p;

				return inner.Concat(on_this);*/
				return Enumerable.Empty<Identifier>();
			}
		}

        public override bool Equals(object obj)
        {
            var x = obj as ComprehensionFor;

            if(x == null)
                return false;

            return this.Left.Equals(x.Left) && this.Target.Equals(x.Target) && this.Body.Equals(x.Body);
        }

        public override int GetHashCode()
        {
            return this.Left.GetHashCode() ^ this.Target.GetHashCode() ^ this.Body.GetHashCode();
        }

        public override void AcceptWalker(AstWalker walker)
		{
			if(walker.Walk(this)){
                Left.AcceptWalker(walker);
                Target.AcceptWalker(walker);
                if(Body != null)
                    Body.AcceptWalker(walker);
			}
			walker.PostWalk(this);
		}

        public override void AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.Walk(this);
        }

        public override string GetText()
        {
            return string.Format("<CompFor: {0} {1} in {2}>", Target.GetText(), Left.GetText(), Body.GetText());
        }

        public override string ToString()
        {
            return GetText();
        }
	}

	public class ComprehensionIf : ComprehensionIter
	{
		/// <summary>
        /// 実行対象の文。
        /// The body that will be executed.
        /// </summary>
        public Expression Condition{
            get{return (Expression)FirstChild;}
		}

		/// <summary>
        /// 実行対象の文。
        /// The body that will be executed.
        /// </summary>
		public ComprehensionIter Body{
            get{return (ComprehensionIter)LastChild;}
		}

        public override NodeType Type{
            get{return NodeType.ComprehensionIf;}
        }

		public ComprehensionIf(Expression test, ComprehensionIter bodyExpr)
		{
            AddChild(test);
            AddChild(bodyExpr);
		}

        public override IEnumerable<Identifier> LocalVariables{
			get{
				return (Body == null) ? Enumerable.Empty<Identifier>() : Body.LocalVariables;
			}
		}

        public override bool Equals(object obj)
        {
            var x = obj as ComprehensionIf;

            if(x == null)
                return false;

            return this.Body.Equals(x.Body) && this.Condition.Equals(x.Condition);
        }

        public override int GetHashCode()
        {
            return this.Body.GetHashCode() ^ this.Condition.GetHashCode();
        }

        public override void AcceptWalker(AstWalker walker)
		{
			if(walker.Walk(this)){
                Condition.AcceptWalker(walker);
                if(Body != null)
                    Body.AcceptWalker(walker);
			}
			walker.PostWalk(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.Walk(this);
        }

        public override string GetText()
        {
            return string.Format("<CompIf: {0} {1}>", Condition.GetText(), Body.GetText());
        }

        public override string ToString()
        {
            return GetText();
        }
	}
}

