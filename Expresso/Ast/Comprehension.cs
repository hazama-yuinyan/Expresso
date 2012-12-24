using System;
using System.Collections.Generic;
using System.Linq;
using Expresso.Builtins;
using Expresso.Helpers;
using Expresso.Interpreter;
using Expresso.Compiler;


namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	public class Comprehension : Expression
	{
		public Expression YieldExpr{get; internal set;}

		public ComprehensionFor Body{get; internal set;}

		public ObjectTypes ObjType{get; internal set;}

		public override NodeType Type
        {
            get { return NodeType.Comprehension; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as Comprehension;

            if (x == null) return false;

            return this.Body.Equals(x.Body) && this.YieldExpr.Equals(x.YieldExpr);
        }

        public override int GetHashCode()
        {
            return this.Body.GetHashCode() ^ this.YieldExpr.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
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
					obj = ExpressoFunctions.MakeList(container);
				else
					obj = ExpressoFunctions.MakeTuple(container);
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

				obj = ExpressoFunctions.MakeDict(keys, values);
			}

			return obj;
        }

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}
	}

	public abstract class ComprehensionIter : Expression
	{
		public abstract IEnumerable<Identifier> LocalVariables{get;}
		internal override object Run(VariableStore varStore){return null;}
		internal abstract IEnumerable<object> Run(VariableStore varStore, Expression yieldExpr);
	}

	public class ComprehensionFor : ComprehensionIter
	{
		/// <summary>
        /// body内で操作対象となるオブジェクトを参照するのに使用する式群。
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
        public List<Expression> LValues { get; internal set; }

        /// <summary>
        /// 操作する対象の式。
        /// The target expression.
        /// </summary>
        public Expression Target { get; internal set; }

		/// <summary>
        /// 実行対象の文。
        /// The body that will be executed.
        /// </summary>
		public ComprehensionIter Body{get; internal set;}

        public override NodeType Type
        {
            get { return NodeType.ComprehensionFor; }
        }

		public override IEnumerable<Identifier> LocalVariables
		{
			get{
				var inner = (Body == null) ? Enumerable.Empty<Identifier>() : Body.LocalVariables;
				var on_this =
					from p in LValues
					select (Identifier)p;

				return inner.Concat(on_this);
			}
		}

        public override bool Equals(object obj)
        {
            var x = obj as ComprehensionFor;

            if (x == null) return false;

            return this.LValues.Equals(x.LValues) && this.Target.Equals(x.Target) && this.Body.Equals(x.Body);
        }

        public override int GetHashCode()
        {
            return this.LValues.GetHashCode() ^ this.Target.GetHashCode() ^ this.Body.GetHashCode();
        }

        internal override IEnumerable<object> Run(VariableStore varStore, Expression yieldExpr)
        {
			IEnumerable<object> iterable = Target.Run(varStore) as IEnumerable<object>;
			if(iterable == null)
				throw new EvalException("Can not evaluate the expression to an iterable object!");

			Identifier[] lvalues = new Identifier[LValues.Count];
			for (int i = 0; i < LValues.Count; ++i) {
				lvalues[i] = LValues[i] as Identifier;
				if(lvalues[i] == null)
					throw new EvalException("The left-hand-side of the \"in\" keyword must yield a lvalue(a referencible value such as variables)");
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
        }

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}
	}

	public class ComprehensionIf : ComprehensionIter
	{
		/// <summary>
        /// 実行対象の文。
        /// The body that will be executed.
        /// </summary>
        public Expression Condition { get; internal set; }

		/// <summary>
        /// 実行対象の文。
        /// The body that will be executed.
        /// </summary>
		public ComprehensionIter Body{get; internal set;}

        public override NodeType Type
        {
            get { return NodeType.ComprehensionIf; }
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

        internal override IEnumerable<object> Run(VariableStore varStore, Expression yieldExpr)
        {
			var cond = Condition.Run(varStore) as Nullable<bool>;
			if(cond == null)
				throw new EvalException("Can not evaluate the expression to a boolean.");

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
        }

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}
	}
}

