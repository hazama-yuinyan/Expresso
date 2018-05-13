using System.Collections.Generic;
using System.Linq;

using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
    /// <summary>
    /// オブジェクト生成式。
    /// Represents an object creation expression.
    /// An object creation expression appears in places where rvalues are expected
    /// and it creates a new object of type `Type` on the heap.
    /// Path '{' { ident ':' Expression } '}' ;
    /// </summary>
    public class ObjectCreationExpression : Expression
    {
        public static readonly Role<FunctionType> CtorTypeRole = new Role<FunctionType>("CtorType", FunctionType.Null);

        #region Null
        public static new readonly ObjectCreationExpression Null = new NullObjectCreationExpression();

        sealed class NullObjectCreationExpression : ObjectCreationExpression
        {
            public override bool IsNull => true;

            public override void AcceptWalker(IAstWalker walker) => walker.VisitNullNode(this);

            public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker) => walker.VisitNullNode(this);

            public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data) => walker.VisitNullNode(this, data);

            internal protected override bool DoMatch(AstNode other, Match match) => other == null || other.IsNull;
        }
        #endregion

        /// <summary>
        /// The type path to create an instance out of.
        /// </summary>
        public AstType TypePath{
            get => GetChildByRole(Roles.Type);
            set => SetChildByRole(Roles.Type, value);
        }

        /// <summary>
        /// Gets all the key-value pairs.
        /// </summary>
        /// <value>The field names.</value>
        public AstNodeCollection<KeyValueLikeExpression> Items => GetChildrenByRole(Roles.KeyValue);

        /// <summary>
        /// Gets or sets the type of the constructor.
        /// Used to resolve the type of the constructor.
        /// </summary>
        /// <value>The type of the ctor.</value>
        public FunctionType CtorType{
            get => GetChildByRole(CtorTypeRole);
            set => SetChildByRole(CtorTypeRole, value);
        }

        protected ObjectCreationExpression()
        {
        }

        public ObjectCreationExpression(AstType path, IEnumerable<PathExpression> fields, IEnumerable<Expression> values, TextLocation end)
            : base(path.StartLocation, end)
        {
            TypePath = path;
            foreach(var item in fields.Zip(values, (field, value) => new KeyValueLikeExpression(field, value)))
                Items.Add(item);
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitObjectCreationExpression(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitObjectCreationExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitObjectCreationExpression(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as ObjectCreationExpression;
            return o != null && TypePath.DoMatch(o.TypePath, match) && Items.DoMatch(o.Items, match);
        }

        #endregion
    }
}

