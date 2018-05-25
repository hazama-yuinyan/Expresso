using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
    /// <summary>
    /// Represents a type path pattern.
    /// Used for C-like enum patterns.
    /// AstType ;
    /// </summary>
    public class TypePathPattern : PatternConstruct
    {
        /// <summary>
        /// Represents the path to the enum member.
        /// </summary>
        /// <value>The type path.</value>
        public AstType TypePath{
            get => GetChildByRole(Roles.Type);
            set => SetChildByRole(Roles.Type, value);
        }

        public TypePathPattern(AstType typePath)
            : base(typePath.StartLocation, typePath.EndLocation)
        {
            TypePath = typePath;
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitTypePathPattern(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitTypePathPattern(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitTypePathPattern(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as TypePathPattern;
            return o != null && TypePath.DoMatch(o.TypePath, match);
        }
    }
}
