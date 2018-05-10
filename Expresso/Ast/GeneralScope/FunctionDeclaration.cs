using System.Collections.Generic;

using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;

namespace Expresso.Ast
{
    /// <summary>
    /// 関数定義。
    /// Represents a function declaration.
    /// A function declaration declares a new function into function namespace.
    /// [ AttributeSection ][ Modifiers ] "def" Name '(' [ Arguments ] ')' [ "->" ReturnType ] '{' Body '}' ;
    /// </summary>
    public class FunctionDeclaration : EntityDeclaration
    {
        public override SymbolKind SymbolKind => SymbolKind.Method;

        /// <summary>
        /// Represents the attribute.
        /// </summary>
        /// <value>The attribute.</value>
        public AttributeSection Attribute{
            get{return GetChildByRole(AttributeRole);}
            set{SetChildByRole(AttributeRole, value);}
        }

        /// <summary>
        /// 仮引数リスト。
		/// The formal parameter list.
        /// It can be empty if the function takes no parameters.
        /// </summary>
        public AstNodeCollection<ParameterDeclaration> Parameters => GetChildrenByRole(Roles.Parameter);

        /// <summary>
        /// 関数本体。
		/// The body of the function.
        /// </summary>
        public BlockStatement Body{
            get{return GetChildByRole(Roles.Body);}
            set{SetChildByRole(Roles.Body, value);}
        }
		
		/// <summary>
		/// Indicates whether the function is static.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is static; otherwise, <c>false</c>.
		/// </value>
        public bool IsStatic => HasModifier(Modifiers.Static);

        public FunctionDeclaration(Identifier ident, IEnumerable<ParameterDeclaration> formalParameters,
                                   BlockStatement body, AstType returnType, AttributeSection attribute, Modifiers modifiers, TextLocation loc)
            : base(loc, body == null ? returnType.EndLocation : body.EndLocation)
		{
            SetChildByRole(Roles.Identifier, ident);
            Attribute = attribute;
            if(formalParameters != null)
                Parameters.AddRange(formalParameters);

            Body = body;
            SetChildByRole(Roles.Type, returnType);
            SetModifiers(this, modifiers);
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitFunctionDeclaration(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitFunctionDeclaration(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitFunctionDeclaration(this, data);
        }

        internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as FunctionDeclaration;
            return o != null && Name == o.Name && Parameters.DoMatch(o.Parameters, match)
                && ReturnType.DoMatch(o.ReturnType, match) && Body.DoMatch(o.Body, match);
        }
    }
}
