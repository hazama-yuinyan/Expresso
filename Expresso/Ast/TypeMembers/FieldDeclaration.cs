using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a field declaration.
    /// A field declaration introduces a new field into the current scope.
    /// Modifiers ("let" | "var") ident [ "(-" Type ] [ '=' Expression ] { ',' ident [ "(-" Type ] [ '=' Expression ] } ';' ;
    /// </summary>
    public class FieldDeclaration : EntityDeclaration
    {
        public override SymbolKind SymbolKind{
            get{
                return SymbolKind.Field;
            }
        }

        // Initializers shouldn't be renamed to Identifiers because of module fields.
        /// <summary>
        /// The initializers that actually initializes the fields.
        /// </summary>
        /// <value>The initializers.</value>
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

        public FieldDeclaration(IEnumerable<PatternWithType> lhs, IEnumerable<Expression> rhs,
            Modifiers modifiers, TextLocation start, TextLocation end)
            : base(start, end)
        {
            rhs = rhs ?? lhs.Select(arg => Expression.Null);
            foreach(var variable in lhs.Zip(rhs, (pattern, expr) => new Tuple<PatternWithType, Expression>(pattern, expr)))
                Initializers.Add(new VariableInitializer(variable.Item1, variable.Item2));

            SetModifiers(this, modifiers);
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
            return o != null && Initializers.DoMatch(o.Initializers, match) && Modifiers == o.Modifiers;
        }
    }
}

