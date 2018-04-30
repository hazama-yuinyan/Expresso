using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;


namespace Expresso.Ast
{
    /// <summary>
    /// 仮引数定義。
    /// Represents a parameter(which appears in function signatures).
    /// A ParameterDeclaration represents a parameter of a function or a method.
    /// Note that it doesn't represent a real argument.
    /// ident [ "(-" Type ] [ "=" Expression ] ;
    /// </summary>
    public class ParameterDeclaration : EntityDeclaration
    {
        public override AstType ReturnType{
            get{return NameToken.Type;}
        }

        public ExpressoTokenNode AssignToken{
            get{return GetChildByRole(Roles.AssignToken);}
        }

        /// <summary>
        /// この引数のデフォルト値。
        /// The optional value for this parameter. It would be a null node if none is specified.
        /// </summary>
        public Expression Option{
            get{return GetChildByRole(Roles.Expression);}
            set{SetChildByRole(Roles.Expression, value);}
        }

        /// <summary>
        /// Represents whether the parameter is variadic or not.
        /// </summary>
        /// <value><c>true</c> if it is variadic; otherwise, <c>false</c>.</value>
        public bool IsVariadic{
            get; set;
        }

        #region implemented abstract members of EntityDeclaration

        public override SymbolKind SymbolKind{
            get{
                return SymbolKind.Parameter;
            }
        }

        #endregion

        public ParameterDeclaration(Identifier identifier, Expression option, bool isVariadic, TextLocation loc)
            : base(loc, option != null ? option.EndLocation : identifier.EndLocation)
        {
            SetChildByRole(Roles.Identifier, identifier);
            Option = option;
            IsVariadic = isVariadic;
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitParameterDeclaration(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitParameterDeclaration(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitParameterDeclaration(this, data);
        }

        internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ParameterDeclaration;
            return o != null && ReturnType.DoMatch(o.ReturnType, match) && MatchString(Name, o.Name)
                                          && Option.DoMatch(o.Option, match) && IsVariadic == o.IsVariadic;
        }
    }
}

