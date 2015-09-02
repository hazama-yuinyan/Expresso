using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents the function type as AST.
    /// In practice it just represents the function signature, so 
    /// </summary>
    public class FunctionType : AstType
    {
        public static readonly Role<AstType> Parameter = new Role<AstType>("Parameter", AstType.Null);

        /// <summary>
        /// Represents the return type.
        /// </summary>
        public AstType ReturnType{
            get{return GetChildByRole(Roles.Type);}
            set{SetChildByRole(Roles.Type, value);}
        }

        /// <summary>
        /// The parameters for the function.
        /// </summary>
        public AstNodeCollection<AstType> Parameters{
            get{return GetChildrenByRole(Parameter);}
        }

        /// <summary>
        /// The identifier of the function.
        /// </summary>
        public Identifier Identifier{
            get{return GetChildByRole(Roles.Identifier);}
            set{SetChildByRole(Roles.Identifier, value);}
        }

        public FunctionType(Identifier ident, AstType returnType, IEnumerable<AstType> parameters)
        {
            Identifier = ident;
            ReturnType = returnType;
            foreach(var param in parameters)
                AddChild(param, Parameter);
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitFunctionType(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitFunctionType(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitFunctionType(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as FunctionType;
            return o != null && ReturnType.DoMatch(o.ReturnType, match)
                && Parameters.DoMatch(o.Parameters, match);
        }

        #endregion

        #region implemented abstract members of AstType

        public override ICSharpCode.NRefactory.TypeSystem.ITypeReference ToTypeReference(NameLookupMode lookupMode, ICSharpCode.NRefactory.TypeSystem.InterningProvider interningProvider = null)
        {
            throw new NotImplementedException();
        }

        public override string Name{
            get{
                return Identifier.Name;
            }
        }

        public override Identifier IdentifierNode{
            get{
                return Identifier;
            }
        }

        #endregion
    }
}

