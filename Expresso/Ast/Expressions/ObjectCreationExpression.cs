using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// オブジェクト生成式。
    /// Represents an object creation expression.
    /// An object creation expression appears in places that rvalues are expected
    /// and it creates a new object of type `Type` on the stack(if used without the "new" keyword)
    /// or on the heap(if the "new" keyword is prepended).
    /// Type '{' { ident ':' Expression } '}'
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

        #region Pattern Placeholder
        public static implicit operator ObjectCreationExpression(Pattern pattern)
        {
            return (pattern != null) ? new PatternPlaceholder(pattern) : null;
        }

        sealed class PatternPlaceholder : ObjectCreationExpression, INode
        {
            readonly Pattern child;

            public PatternPlaceholder(Pattern child)
            {
                this.child = child;
            }

            public override NodeType NodeType{
                get{
                    return NodeType.Pattern;
                }
            }

            public override void AcceptWalker(IAstWalker walker)
            {
                walker.VisitPatternPlaceholder(this);
            }

            public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
            {
                return walker.VisitPatternPlaceholder(this);
            }

            public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
            {
                return walker.VisitPatternPlaceholder(this, data);
            }

            internal protected override bool DoMatch(AstNode other, Match match)
            {
                return child.DoMatch(other, match);
            }

            bool INode.DoMatchCollection(Role role, INode pos, Match match, BacktrackingInfo backtrackingInfo)
            {
                return child.DoMatchCollection(role, pos, match, backtrackingInfo);
            }
        }
        #endregion

        /// <summary>
        /// Gets all the field names.
        /// </summary>
        /// <value>The field names.</value>
        public AstNodeCollection<Identifier> FieldNames{
            get{return GetChildrenByRole(Roles.Identifier);}
        }

        public AstNodeCollection<ExpressoTokenNode> ColonToken{
            get{return GetChildrenByRole(Roles.ColonToken);}
        }

        /// <summary>
        /// Gets all the values.
        /// </summary>
        /// <value>The values.</value>
        public AstNodeCollection<Expression> Values{
            get{return GetChildrenByRole(Roles.Expression);}
        }

        public ObjectCreationExpression(IEnumerable<Identifier> fields, IEnumerable<Expression> values)
        {
            foreach(var pair in fields.Zip(values, (field, value) => new Tuple<Identifier, Expression>(field, value))){
                AddChild(pair.Item1, Roles.Identifier);
                AddChild(pair.Item2, Roles.Expression);
            }
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
            return o != null && FieldNames.DoMatch(o.FieldNames, match) && Values.DoMatch(o.Values, match);
        }

        #endregion
    }
}

