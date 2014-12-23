using System;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a variable initialization.
    /// </summary>
    public class VariableInitializer : AstNode
    {
        #region Null
        public static readonly VariableInitializer Null = new NullVariableInitializer();
        sealed class NullVariableInitializer : VariableInitializer
        {
            public override NodeType NodeType{
                get{
                    return NodeType.Unknown;
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

        #region PatternPlaceholder
        public static implicit operator VariableInitializer(Pattern pattern)
        {
            return (pattern != null) ? new PatternPlaceholder(pattern) : null;
        }

        sealed class PatternPlaceholder : VariableInitializer, INode
        {
            readonly Pattern child;

            public PatternPlaceholder(Pattern child)
            {
                this.child = child;
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

        public override NodeType NodeType{
            get{
                return NodeType.Unknown;
            }
        }

        public string Name{
            get{return GetChildByRole(Roles.Identifier).Name;}
        }

        public Identifier NameToken{
            get{return GetChildByRole(Roles.Identifier);}
        }

        public ExpressoTokenNode AssignToken{
            get{return GetChildByRole(Roles.AssignToken);}
        }

        public Expression Initializer{
            get{return GetChildByRole(Roles.Expression);}
        }

        public VariableInitializer(Identifier name, Expression initializer = null)
        {
            SetChildByRole(Roles.Identifier, name);
            SetChildByRole(Roles.Expression, initializer ?? Expression.Null);
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitVariableInitializer(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitVariableInitializer(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitVariableInitializer(this, data);
        }

        internal protected override bool DoMatch(AstNode other, Match match)
        {
            var o = other as VariableInitializer;
            return o != null && MatchString(Name, o.Name) && Initializer.DoMatch(o.Initializer);
        }
    }
}

