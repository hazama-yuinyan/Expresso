using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    using ExpressoModifiers = Expresso.Ast.Modifiers;

    /// <summary>
    /// 識別子。
	/// Reperesents a symbol.
    /// Identifiers should not be associated with AstType nodes if they just refer to types
    /// because doing so creates reference cycles and thus causes problems.
    /// But there is an exception where Identifiers will be associated with AstType nodes
    /// if there are aliases for the names the Identifiers refer to.
    /// </summary>
    public class Identifier : AstNode
    {
        #region Null
        public static readonly new Identifier Null = new NullIdentifier();

        sealed class NullIdentifier : Identifier
        {
            public override bool IsNull{
                get{
                    return true;
                }
            }

            public override void AcceptWalker(IAstWalker walker)
            {
                walker.VisitNullNode(this);
            }

            public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
            {
                return walker.VisitNullNode(this);
            }

            public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
            {
                return walker.VisitNullNode(this, data);
            }

            internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
            {
                return other == null || other.IsNull;
            }
        }
        #endregion

		readonly string name;

        /// <summary>
        /// 識別子名。
		/// The name of the identifier.
        /// </summary>
        public string Name{
            get{return name;}
		}

        /// <summary>
        /// Gets or sets the identifier id. This id represents the identity or uniqueness of that node
        /// within a whole program.
        /// If some 2 Identifier nodes have the same id, that means that the 2 nodes refer to the same value.
        /// This will get set during name binding and type validity check.
        /// </summary>
        /// <remarks>0 is considered invalid for IdentifierId.</remarks>
        public uint IdentifierId{
            get; internal set;
        }

		/// <summary>
		/// 変数の型。
		/// The type of the variable.
		/// </summary>
        public AstType Type{
            get{return GetChildByRole(Roles.Type);}
            set{SetChildByRole(Roles.Type, value);}
		}

        /// <summary>
        /// The modifiers this identifier has.
        /// </summary>
        /// <value>The modifiers.</value>
        public ExpressoModifiers Modifiers{
            get{return EntityDeclaration.GetModifiers(this);}
            set{EntityDeclaration.SetModifiers(this, value);}
        }

        public override NodeType NodeType{
            get{return NodeType.Unknown;}
        }

        protected Identifier()
        {
            IdentifierId = 0;
        }

        public Identifier(string name, ExpressoModifiers modifiers, TextLocation loc)
            : base(loc, new TextLocation(loc.Line, loc.Column + name.Length))
		{
            this.name = name;
            Modifiers = modifiers;
            IdentifierId = 0;
		}

        public Identifier(string name, AstType type, ExpressoModifiers modifiers, TextLocation loc)
            : base(loc, type.EndLocation)
        {
            this.name = name;
            Modifiers = modifiers;
            Type = type;
            IdentifierId = 0;
        }

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitIdentifier(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitIdentifier(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitIdentifier(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as Identifier;
            return o != null && MatchString(Name, o.Name) && Type.DoMatch(o.Type, match);
        }

        #endregion
    }
}
