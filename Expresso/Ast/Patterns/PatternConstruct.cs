using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
    /// <summary>
    /// The base class for all pattern constructs.
    /// </summary>
    public abstract class PatternConstruct : AstNode
    {
        #region Null
        public static new PatternConstruct Null = new NullPatternConstruct();

        sealed class NullPatternConstruct : PatternConstruct
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

            protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
            {
                return other == null || other.IsNull;
            }
        }
        #endregion

        #region PatternPlaceholder
        public static implicit operator PatternConstruct(Pattern pattern)
        {
            return (pattern != null) ? new PatternPlaceholder(pattern) : null;
        }

        sealed class PatternPlaceholder : PatternConstruct, INode
        {
            readonly Pattern child;

            public PatternPlaceholder(Pattern child)
            {
                this.child = child;
            }

            public override void AcceptWalker(IAstWalker walker)
            {
                walker.VisitPatternPlaceholder(this, child);
            }

            public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
            {
                return walker.VisitPatternPlaceholder(this, child);
            }

            public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
            {
                return walker.VisitPatternPlaceholder(this, child, data);
            }

            protected internal override bool DoMatch(AstNode other, Match match)
            {
                return child.DoMatch(other, match);
            }

            bool INode.DoMatchCollection(Role role, INode pos, Match match, BacktrackingInfo backtrackingInfo)
            {
                return child.DoMatchCollection(role, pos, match, backtrackingInfo);
            }
        }
        #endregion

        #region implemented abstract members of AstNode

        public override NodeType NodeType{
            get{
                return NodeType.Expression;
            }
        }

        #endregion

        #region Factory methods
        static internal WildcardPattern MakeWildcardPattern()
        {
            return new WildcardPattern();
        }

        static internal IdentifierPattern MakeIdentifierPattern(string name, PatternConstruct inner)
        {
            var type = new PlaceholderType(TextLocation.Empty);
            return new IdentifierPattern(AstNode.MakeIdentifier(name, type), inner);
        }

        static internal ValueBindingPattern MakeValueBindingPattern(PatternConstruct inner, bool isConst)
        {
            return new ValueBindingPattern(inner, isConst);
        }

        static internal CollectionPattern MakeCollectionPattern(IEnumerable<PatternConstruct> items,
            bool isVector)
        {
            return new CollectionPattern(items, isVector);
        }

        static internal DestructuringPattern MakeDestructuringPattern(PathExpression path,
            IEnumerable<PatternConstruct> inners)
        {
            return new DestructuringPattern(path, inners);
        }

        static internal TuplePattern MakeTuplePattern(List<PatternConstruct> inners)
        {
            return new TuplePattern(inners);
        }

        static internal ExpressionPattern MakeExpressionPattern(Expression inner)
        {
            return new ExpressionPattern(inner);
        }
        #endregion
    }
}

