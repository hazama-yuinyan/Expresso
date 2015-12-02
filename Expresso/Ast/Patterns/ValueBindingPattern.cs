using System;
using System.Collections.Generic;
using System.Linq;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a value binding pattern.
    /// A ValueBindingPattern introduces a new variable into the current scope and captures a value into that variable.
    /// ( "let" | "var" ) PatternConstruct ;
    /// </summary>
    public class ValueBindingPattern : PatternConstruct
    {
        /// <summary>
        /// Represents the inner pattern.
        /// </summary>
        public AstNodeCollection<VariableInitializer> Variables{
            get{return GetChildrenByRole(Roles.Variable);}
        }

        /// <summary>
        /// Represents the modifiers that describe the properties of inner patterns.
        /// </summary>
        public Modifiers Modifiers{
            get{return EntityDeclaration.GetModifiers(this);}
            set{EntityDeclaration.SetModifiers(this, value);}
        }

        public ValueBindingPattern(IEnumerable<VariableInitializer> inits, Modifiers modifiers)
        {
            Modifiers = modifiers;
            Variables.AddRange(inits);
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitValueBindingPattern(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitValueBindingPattern(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitValueBindingPattern(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ValueBindingPattern;
            return o != null && Variables.DoMatch(o.Variables, match)
                && Modifiers == o.Modifiers;
        }

        #endregion
    }
}

