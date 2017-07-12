using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
    /// <summary>
    /// Represents the closure literal expression.
    /// A closure literal expression defines a new closure.
    /// '|' { Identifiers } '|' [ "->" ReturnType ] ( Expression | Block ) ;
    /// </summary>
    public class ClosureLiteralExpression : Expression
    {
        /// <summary>
        /// Represents the parameters on this closure.
        /// </summary>
        /// <value>The parameters.</value>
        public AstNodeCollection<ParameterDeclaration> Parameters{
            get{return GetChildrenByRole(Roles.Parameter);}
        }

        /// <summary>
        /// The return type of this closure.
        /// </summary>
        /// <value>The type of the return.</value>
        public AstType ReturnType{
            get{return GetChildByRole(Roles.Type);}
            set{SetChildByRole(Roles.Type, value);}
        }

        /// <summary>
        /// The body block.
        /// The body block can be an expression or a block.
        /// </summary>
        /// <value>The body.</value>
        public BlockStatement Body{
            get{return GetChildByRole(Roles.Body);}
            set{SetChildByRole(Roles.Body, value);}
        }

        public ClosureLiteralExpression(IEnumerable<ParameterDeclaration> parameters, AstType returnType, BlockStatement body, TextLocation loc)
            : base(loc, body.EndLocation)
        {
            Parameters.AddRange(parameters);
            ReturnType = returnType;
            Body = body;
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitClosureLiteralExpression(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitClosureLiteralExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitClosureLiteralExpression(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as ClosureLiteralExpression;
            return o != null && Parameters.DoMatch(o.Parameters, match) && ReturnType.DoMatch(o.ReturnType, match) && Body.DoMatch(o.Body, match);
        }
    }
}
