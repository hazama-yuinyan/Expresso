using System;
using System.ComponentModel;
using System.Collections.Generic;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a field declaration.
    /// ("let" | "var") ident [ "(-" Type ] { ',' ident [ "(-" Type ] } ';'
    /// </summary>
    public class FieldDeclaration : EntityDeclaration
    {
        public override ICSharpCode.NRefactory.TypeSystem.SymbolKind SymbolKind{
            get{
                return SymbolKind.Field;
            }
        }

        public AstNodeCollection<VariableInitializer> Initializers{
            get{return GetChildrenByRole(Roles.Variable);}
        }

        // Hide Name and NameToken properties from users; the actual field names 
        // are stored in VariableInitializers
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string Name{
            get{
                return string.Empty;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Identifier NameToken{
            get{
                return Identifier.Null;
            }
        }

        public FieldDeclaration(IEnumerable<VariableInitializer> variables)
        {
            foreach(var variable in variables)
                AddChild(variable, Roles.Variable);
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitFieldDeclaration(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitFieldDeclaration(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitFieldDeclaration(this, data);
        }

        internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as FieldDeclaration;
            return o != null && ReturnType.DoMatch(o.ReturnType, match) && Initializers.DoMatch(o.Initializers, match);
        }
    }
}

