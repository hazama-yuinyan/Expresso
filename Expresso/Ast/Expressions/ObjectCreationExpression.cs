using System;
using System.Collections.Generic;
using System.Linq;

using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// オブジェクト生成式。
    /// Represents an object creation expression.
    /// An object creation expression appears in places where rvalues are expected
    /// and it creates a new object of type `Type` on the stack(if used without the "new" keyword)
    /// or on the heap(if the "new" keyword is preceded).
    /// Path '{' { ident ':' Expression } '}' ;
    /// </summary>
    public class ObjectCreationExpression : Expression
    {
        #region Null
        public static new readonly ObjectCreationExpression Null = new NullObjectCreationExpression();

        sealed class NullObjectCreationExpression : ObjectCreationExpression
        {
            public override bool IsNull{
                get{
                    return true;
                }
            }

            public override void AcceptWalker(IAstWalker walker)
            {
                walker.VisitNullNode(this);
            }

            public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
            {
                return walker.VisitNullNode(this);
            }

            public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
            {
                return walker.VisitNullNode(this, data);
            }

            internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
            {
                return other == null || other.IsNull;
            }
        }
        #endregion

        /// <summary>
        /// The type path to create an instance out of.
        /// </summary>
        public AstType TypePath{
            get{return GetChildByRole(Roles.Type);}
            set{SetChildByRole(Roles.Type, value);}
        }

        /// <summary>
        /// Gets all the key-value pairs.
        /// </summary>
        /// <value>The field names.</value>
        public AstNodeCollection<KeyValueLikeExpression> Items{
            get{return GetChildrenByRole(Roles.KeyValue);}
        }

        protected ObjectCreationExpression()
        {
        }

        public ObjectCreationExpression(AstType path, IEnumerable<PathExpression> fields, IEnumerable<Expression> values)
        {
            TypePath = path;
            foreach(var item in fields.Zip(values, (field, value) => new KeyValueLikeExpression(field, value)))
                Items.Add(item);
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitObjectCreationExpression(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitObjectCreationExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitObjectCreationExpression(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ObjectCreationExpression;
            return o != null && TypePath.DoMatch(o.TypePath, match) && Items.DoMatch(o.Items, match);
        }

        #endregion
    }
}

