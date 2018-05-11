using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a variable initialization.
    /// A variable initialization represents a variable and an initialization.
    /// Note that the syntax itself is the same as that of ParameterDeclaration.
    /// Pattern ( [ "(-" Type ] [ '=' Expression ] ) ;
    /// </summary>
    public class VariableInitializer : AstNode
    {
        #region Null
        public static readonly new VariableInitializer Null = new NullVariableInitializer();

        sealed class NullVariableInitializer : VariableInitializer
        {
            public override bool IsNull => true;

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

            protected internal override bool DoMatch (AstNode other, Match match)
            {
                return other == null || other.IsNull;
            }
        }
        #endregion
        public override NodeType NodeType => NodeType.Unknown;

        /// <summary>
        /// Convenient property for retrieving the inner identifier pattern.
        /// </summary>
        /// <value>The name.</value>
        public string Name{
            get{
                var pattern = Pattern;
                if(pattern is PatternWithType inner && inner.Pattern is IdentifierPattern ident_pat)
                    return ident_pat.Identifier.Name;
                else
                    return null;
            }
        }

        public Identifier NameToken{
            get{
                var pattern = Pattern;
                if(pattern is PatternWithType inner && inner.Pattern is IdentifierPattern ident_pat)
                    return ident_pat.Identifier;
                else
                    return null;
            }
        }

        /// <summary>
        /// Represents the pattern.
        /// </summary>
        /// <value>The name token.</value>
        public PatternWithType Pattern{
            get => GetChildByRole(Roles.TypedPattern);
            set => SetChildByRole(Roles.TypedPattern, value);
        }

        public ExpressoTokenNode AssignToken => GetChildByRole(Roles.AssignToken);

        /// <summary>
        /// Represents the initialization code.
        /// </summary>
        /// <value>The initializer.</value>
        public Expression Initializer{
            get => GetChildByRole(Roles.Expression);
            set => SetChildByRole(Roles.Expression, value);
        }

        protected VariableInitializer()
        {
        }

        public VariableInitializer(PatternWithType pattern, Expression initializer = null)
            : base(pattern.StartLocation, initializer == null ? pattern.EndLocation : initializer.EndLocation)
        {
            Pattern = pattern;
            Initializer = initializer;
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
            return o != null && Pattern.DoMatch(o.Pattern, match) && Initializer.DoMatch(o.Initializer, match);
        }
    }
}

