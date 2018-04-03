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
                : base(default(TextLocation), default(TextLocation))
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

        protected PatternConstruct()
        {
        }

        protected PatternConstruct(TextLocation startLoc, TextLocation endLoc)
            : base(startLoc, endLoc)
        {
        }

        #endregion

        #region Factory methods
        public static WildcardPattern MakeWildcardPattern(TextLocation loc = default(TextLocation))
        {
            return new WildcardPattern(loc);
        }

        public static IdentifierPattern MakeIdentifierPattern(string name, AstType type,
                                                              PatternConstruct inner = null, TextLocation loc = default(TextLocation))
        {
            // type.Clone is needed because Enumerator closures will be executed over twice
            return new IdentifierPattern(AstNode.MakeIdentifier(name, type.Clone(), Modifiers.None, loc), inner);
        }

        public static IdentifierPattern MakeIdentifierPattern(Identifier ident, PatternConstruct inner = null)
        {
            return new IdentifierPattern(ident, inner);
        }

        public static CollectionPattern MakeCollectionPattern(IEnumerable<PatternConstruct> items, bool isVector)
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

        public static IgnoringRestPattern MakeIgnoringRestPattern(TextLocation loc = default(TextLocation))
        {
            return new IgnoringRestPattern(loc);
        }

        public static KeyValuePattern MakeKeyValuePattern(string key, PatternConstruct value)
        {
            return new KeyValuePattern(AstNode.MakeIdentifier(key, new PlaceholderType(TextLocation.Empty)), value);
        }

        public static KeyValuePattern MakeKeyValuePattern(Identifier key, PatternConstruct value)
        {
            return new KeyValuePattern(key, value);
        }

        public static PatternWithType MakePatternWithType(PatternConstruct pattern, AstType type)
        {
            return new PatternWithType(pattern, type);
        }
        #endregion
    }
}

