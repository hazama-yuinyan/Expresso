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
        public static WildcardPattern MakeWildcardPattern()
        {
            return new WildcardPattern();
        }

        public static IdentifierPattern MakeIdentifierPattern(string name, PatternConstruct inner)
        {
            var type = new PlaceholderType(TextLocation.Empty);
            return new IdentifierPattern(AstNode.MakeIdentifier(name, type), inner);
        }

        public static ValueBindingPattern MakeValueBindingPattern(PatternConstruct inner, Modifiers modifiers)
        {
            return new ValueBindingPattern(inner, modifiers);
        }

        public static CollectionPattern MakeCollectionPattern(IEnumerable<PatternConstruct> items,
            bool isVector)
        {
            return new CollectionPattern(items, isVector);
        }

        public static CollectionPattern MakeCollectionPattern(bool isVector, params PatternConstruct[] items)
        {
            return new CollectionPattern(items, isVector);
        }

        public static DestructuringPattern MakeDestructuringPattern(AstType typePath,
            IEnumerable<PatternConstruct> inners)
        {
            return new DestructuringPattern(typePath, inners);
        }

        public static DestructuringPattern MakeDestructuringPattern(AstType typePath,
            params PatternConstruct[] inners)
        {
            return new DestructuringPattern(typePath, inners);
        }

        public static TuplePattern MakeTuplePattern(IEnumerable<PatternConstruct> inners)
        {
            return new TuplePattern(inners);
        }

        public static TuplePattern MakeTuplePattern(params PatternConstruct[] inners)
        {
            return new TuplePattern(inners);
        }

        public static ExpressionPattern MakeExpressionPattern(Expression inner)
        {
            return new ExpressionPattern(inner);
        }
        #endregion
    }
}

