using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    public static class Roles
    {
        public static readonly Role<AstNode> Root = AstNode.RootRole;

        public static readonly Role<AttributeNode> Attribute =
            new Role<AttributeNode>("Attribute");
        public static readonly Role<Identifier> Identifier =
            new Role<Identifier>("Identifier", Ast.Identifier.Null);
        public static readonly Role<PathExpression> Path =
            new Role<PathExpression>("Path", PathExpression.Null);
        public static readonly Role<BlockStatement> Body =
            new Role<BlockStatement>("Block", BlockStatement.Null);
        public static readonly Role<ParameterDeclaration> Parameter =
            new Role<ParameterDeclaration>("Parameter");
        public static readonly Role<Expression> Argument =
            new Role<Expression>("Argument", Ast.Expression.Null);
        public static readonly Role<KeyValueLikeExpression> KeyValue =
            new Role<KeyValueLikeExpression>("KeyValue");
        public static readonly Role<AstType> Type = new Role<AstType>("Type", AstType.Null);
        public static readonly Role<AstType> BaseType = new Role<AstType>("BaseType", AstType.Null);
        public static readonly Role<Expression> Expression =
            new Role<Expression>("Expression", Ast.Expression.Null);
        public static readonly Role<Expression> TargetExpression =
            new Role<Expression>("Target", Ast.Expression.Null);
        public static readonly Role<AstType> TypeArgument =
            new Role<AstType>("TypeArgument", AstType.Null);
        public static readonly Role<Statement> EmbeddedStatement =
            new Role<Statement>("EmbeddedStatement", Statement.Null);
        public static readonly Role<EntityDeclaration> TypeMember =
            new Role<EntityDeclaration>("TypeMember");
        public static readonly Role<VariableInitializer> Variable =
            new Role<VariableInitializer>("Variable", VariableInitializer.Null);
        public static readonly Role<PatternConstruct> Pattern =
            new Role<PatternConstruct>("Pattern", PatternConstruct.Null);
        public static readonly Role<SimpleType> GenericType =
            new Role<SimpleType>("GenericType", SimpleType.Null);
        public static readonly Role<PatternWithType> TypedPattern = 
            new Role<PatternWithType>("TypedPattern", PatternWithType.Null);

        // some pre-defined roles for most used punctuations
        public static readonly TokenRole LParenthesisToken = new TokenRole("(", ExpressoTokenNode.Null);
        public static readonly TokenRole RParenthesisToken = new TokenRole(")", ExpressoTokenNode.Null);
        public static readonly TokenRole LBracketToken = new TokenRole("[", ExpressoTokenNode.Null);
        public static readonly TokenRole RBracketToken = new TokenRole("]", ExpressoTokenNode.Null);
        public static readonly TokenRole LBraceToken = new TokenRole("{", ExpressoTokenNode.Null);
        public static readonly TokenRole RBraceToken = new TokenRole("}", ExpressoTokenNode.Null);
        public static readonly TokenRole LChevronToken = new TokenRole("<", ExpressoTokenNode.Null);
        public static readonly TokenRole RChevronToken = new TokenRole(">", ExpressoTokenNode.Null);
        public static readonly TokenRole CommaToken = new TokenRole(",", ExpressoTokenNode.Null);
        public static readonly TokenRole DotToken = new TokenRole(".", ExpressoTokenNode.Null);
        public static readonly TokenRole SemicolonToken = new TokenRole(";", ExpressoTokenNode.Null);
        public static readonly TokenRole AmpersandToken = new TokenRole("&", ExpressoTokenNode.Null);
        public static readonly TokenRole AssignToken = new TokenRole("=", ExpressoTokenNode.Null);
        public static readonly TokenRole PlusToken = new TokenRole("+", ExpressoTokenNode.Null);
        public static readonly TokenRole MinusToken = new TokenRole("-", ExpressoTokenNode.Null);
        public static readonly TokenRole ColonToken = new TokenRole(":", ExpressoTokenNode.Null);
        public static readonly TokenRole RangeToken = new TokenRole("..", ExpressoTokenNode.Null);
        public static readonly TokenRole InclusiveRangeToken = new TokenRole("...", ExpressoTokenNode.Null);
        public static readonly TokenRole IncludeToken = new TokenRole("(-", ExpressoTokenNode.Null);
        public static readonly TokenRole ReturnToken = new TokenRole("->", ExpressoTokenNode.Null);
        public static readonly TokenRole IfToken = new TokenRole("if", ExpressoTokenNode.Null);
        public static readonly TokenRole ForToken = new TokenRole("for", ExpressoTokenNode.Null);
        public static readonly TokenRole InToken = new TokenRole("in", ExpressoTokenNode.Null);
        public static readonly TokenRole AsToken = new TokenRole("as", ExpressoTokenNode.Null);
        public static readonly Role<CommentNode> CommentRole = new Role<CommentNode>("Comment");

    }
}

