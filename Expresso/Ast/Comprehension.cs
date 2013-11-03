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
	using CSharpExpr = System.Linq.Expressions.Expression;

	/// <summary>
	/// Represents a list comprehension, which is syntactic sugar for sequence initialization.
	/// Consider an expression, [x for x in [0..100]].
	/// Which is equivalent in functionality to the statement "for(let x in [0..100]) yield x;"
	/// </summary>
	public class Comprehension : Expression
	{
		readonly Expression item;
		readonly ComprehensionFor body;
		ObjectTypes type;

		public Expression Item{
			get{return item;}
		}

		public ComprehensionFor Body{
			get{return body;}
		}

		public ObjectTypes ObjType{
			get{return type;}
		}

		public override NodeType Type
        {
            get { return NodeType.Comprehension; }
        }

		public Comprehension(Expression itemExpr, ComprehensionFor bodyExpr, ObjectTypes objType)
		{
			item = itemExpr;
			body = bodyExpr;
			type = objType;
		}

        public override bool Equals(object obj)
        {
            var x = obj as Comprehension;

            if (x == null) return false;

            return body.Equals(x.body) && item.Equals(x.item);
        }

        public override int GetHashCode()
        {
            return body.GetHashCode() ^ item.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			var child_store = new VariableStore{Parent = varStore};
			foreach(var local in Body.LocalVariables)
				child_store.Add(local.Offset, ImplementationHelpers.GetDefaultValueFor(local.ParamType.ObjType));

			object obj = null;

			if(ObjType == ObjectTypes.LIST || ObjType == ObjectTypes.TUPLE){
				var container = new List<object>();

				foreach(var result in Body.Run(child_store, YieldExpr)){
					if(result != null)
						container.Add(result);
				}

				if(ObjType == ObjectTypes.LIST)
					obj = ExpressoOps.MakeList(container);
				else
					obj = ExpressoOps.MakeTuple(container);
			}else if(ObjType == ObjectTypes.DICT){
				var keys = new List<object>();
				var values = new List<object>();

				var i = 0;
				foreach(var result in Body.Run(varStore, YieldExpr)){
					if(result != null){
						if(i % 2 == 0)
							keys.Add(result);
						else
							values.Add(result);
					}
				}

				obj = ExpressoOps.MakeDict(keys, values);
			}

			return obj;
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				item.Walk(walker);
				body.Walk(walker);
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
		readonly SequenceExpression left;
		readonly Expression target;
		readonly ComprehensionIter body;

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
			get{return left;}
		}

        /// <summary>
        /// 操作する対象の式。
        /// The target expression.
        /// </summary>
        public Expression Target{
			get{return target;}
		}

		/// <summary>
        /// 実行対象の文。
        /// The body that will be executed.
        /// </summary>
		public ComprehensionIter Body{
			get{return body;}
		}

        public override NodeType Type
        {
            get { return NodeType.ComprehensionFor; }
        }

		public ComprehensionFor(SequenceExpression lhs, Expression targetExpr, ComprehensionIter bodyExpr)
		{
			left = lhs;
			target = targetExpr;
			body = bodyExpr;
		}

		public override IEnumerable<Identifier> LocalVariables
		{
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

            if (x == null) return false;

            return this.Left.Equals(x.Left) && this.Target.Equals(x.Target) && this.Body.Equals(x.Body);
        }

        public override int GetHashCode()
        {
            return this.Left.GetHashCode() ^ this.Target.GetHashCode() ^ this.Body.GetHashCode();
        }

        /*internal override IEnumerable<object> Run(VariableStore varStore, Expression yieldExpr)
        {
			IEnumerable<object> iterable = Target.Run(varStore) as IEnumerable<object>;
			if(iterable == null)
				throw ExpressoOps.InvalidTypeError("Can not evaluate the expression to an iterable object!");

			Identifier[] lvalues = new Identifier[LValues.Count];
			for (int i = 0; i < LValues.Count; ++i) {
				lvalues[i] = LValues[i] as Identifier;
				if(lvalues[i] == null)
					throw ExpressoOps.ReferenceError("The left-hand-side of the \"in\" keyword must yield a lvalue(a referencible value such as variables)");
			}

			var enumerator = iterable.GetEnumerator();
			while(enumerator.MoveNext()) {
				foreach (var lvalue in lvalues) {
					var val = enumerator.Current;
					varStore.Assign(lvalue.Level, lvalue.Offset, val);
				}

				if(Body == null){
					yield return yieldExpr.Run(varStore);
				}else{
					foreach(var result in Body.Run(varStore, yieldExpr))
						yield return result;
				}
			}
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				left.Walk(walker);
				target.Walk(walker);
				if(body != null)
					body.Walk(walker);
			}
			walker.PostWalk(this);
		}
	}

	public class ComprehensionIf : ComprehensionIter
	{
		readonly Expression condition;
		readonly ComprehensionIter body;

		/// <summary>
        /// 実行対象の文。
        /// The body that will be executed.
        /// </summary>
        public Expression Condition{
			get{return condition;}
		}

		/// <summary>
        /// 実行対象の文。
        /// The body that will be executed.
        /// </summary>
		public ComprehensionIter Body{
			get{return body;}
		}

        public override NodeType Type
        {
            get { return NodeType.ComprehensionIf; }
        }

		public ComprehensionIf(Expression test, ComprehensionIter bodyExpr)
		{
			condition = test;
			body = bodyExpr;
		}

		public override IEnumerable<Identifier> LocalVariables {
			get {
				return (Body == null) ? Enumerable.Empty<Identifier>() : Body.LocalVariables;
			}
		}

        public override bool Equals(object obj)
        {
            var x = obj as ComprehensionIf;

            if (x == null) return false;

            return this.Body.Equals(x.Body) && this.Condition.Equals(x.Condition);
        }

        public override int GetHashCode()
        {
            return this.Body.GetHashCode() ^ this.Condition.GetHashCode();
        }

        /*internal override IEnumerable<object> Run(VariableStore varStore, Expression yieldExpr)
        {
			var cond = Condition.Run(varStore) as Nullable<bool>;
			if(cond == null)
				throw ExpressoOps.InvalidTypeError("Can not evaluate the expression to a boolean.");

			if((bool)cond){
				if(Body == null){
					yield return yieldExpr.Run(varStore);
				}else{
					foreach(var result in Body.Run(varStore, yieldExpr))
						yield return result;
				}
			}else{
				yield return null;
			}
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				condition.Walk(walker);
				if(body != null)
					body.Walk(walker);
			}
			walker.PostWalk(this);
		}
	}
}

