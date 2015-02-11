using System.Collections.Generic;
using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
	/// <summary>
	/// シーケンス(リストや連想配列など)のリテラル式。
	/// Represents the literal form of a sequence initialization.
	/// </summary>
    public class SequenceInitializer : Expression
	{
		/// <summary>
        /// シーケンス生成に使用する式群。
		/// Expressions generating each element of a sequence object.
        /// </summary>
        public AstNodeCollection<Expression> Items{
            get{return GetChildrenByRole(Roles.Expression);}
		}
		
		/// <summary>
		/// この式群を評価した結果生成されるオブジェクトのタイプ。
		/// The type of sequence object generated by this node.
		/// </summary>
        public AstType ObjectType{
            get{return GetChildByRole(Roles.Type);}
            set{SetChildByRole(Roles.Type, value);}
		}

        public SequenceInitializer(AstType objType, IEnumerable<Expression> seqItems)
		{
            ObjectType = objType;
            if(seqItems != null){
                foreach(var item in seqItems)
                    AddChild(item, Roles.Expression);
            }
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitSequenceInitializer(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitSequenceInitializer(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitSequenceInitializer(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as SequenceInitializer;
            return o != null && ObjectType.DoMatch(o.ObjectType, match) && Items.DoMatch(o.Items, match);
        }

        #endregion
	}
}

