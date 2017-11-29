using System;
using System.Collections.Generic;

using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;
using System.Text;

namespace Expresso.Ast
{
    /// <summary>
    /// 関数定義。
    /// Represents a function declaration.
    /// A function declaration declares a new function into function namespace.
    /// [ Modifiers ] "def" Name '(' [ Arguments ] ')' [ "->" ReturnType ] '{' Body '}' ;
    /// </summary>
    public class FunctionDeclaration : EntityDeclaration
    {
        public override SymbolKind SymbolKind{
            get{
                return SymbolKind.Method;
            }
        }

        /// <summary>
        /// 仮引数リスト。
		/// The formal parameter list.
        /// It can be empty if the function takes no parameters.
        /// </summary>
        public AstNodeCollection<ParameterDeclaration> Parameters{
            get{return GetChildrenByRole(Roles.Parameter);}
		}

        /// <summary>
        /// 関数本体。
		/// The body of the function.
        /// </summary>
        public BlockStatement Body{
            get{return GetChildByRole(Roles.Body);}
            set{SetChildByRole(Roles.Body, value);}
        }
		
		/// <summary>
		/// このクロージャが定義された時の環境。
		/// The environment in which the function is defined. It can be null if the function isn't a closure.
		/// </summary>
        //public Stack<object> Environment{get; internal set;}

        public TextLocation Header{get; internal set;}

		internal bool HasReturn{get; set;}

		/// <summary>
		/// Indicates whether the function is static.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is static; otherwise, <c>false</c>.
		/// </value>
		public bool IsStatic{
            get{
                return HasModifier(Modifiers.Static);
            }
        }

        public FunctionDeclaration(Identifier ident, IEnumerable<ParameterDeclaration> formalParameters,
            BlockStatement body, AstType returnType, Modifiers modifiers, TextLocation loc)
            : base(loc, body == null ? returnType.EndLocation : body.EndLocation)
		{
            SetChildByRole(Roles.Identifier, ident);
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
