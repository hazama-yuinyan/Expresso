using System;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    public static class Roles
    {
        public static readonly Role<AstNode> Root = AstNode.RootRole;

        public static readonly Role<Identifier> Identifier = new Role<Identifier>("Identifier");
        public static readonly Role<BlockStatement> Body = new Role<BlockStatement>("Block", BlockStatement.Null);
        public static readonly Role<ParameterDeclaration> Parameter = new Role<ParameterDeclaration>("Parameter");
        public static readonly Role<Expression> Argument = new Role<Expression>("Argument", Expression.NullObject);
        public static readonly Role<AstType> Type = new Role<AstType>("Type", AstType.Null);
        public static readonly Role<Expression> Expression = new Role<Expression>("Expression", Expression.NullObject);
        public static readonly Role<Expression> TargetExpression = new Role<Expression>("Target", Expression.NullObject);
        public static readonly Role<AstType> TypeArgument = new Role<AstType>("TypeArgument", AstType.Null);
        //public static readonly Role<TypeParameterDeclaration> 
        public static readonly Role<VariableInitializer> Variable = new Role<VariableInitializer>("Variable", VariableInitializer.Null);
        public static readonly Role<Statement> EmbeddedStatement = new Role<Statement>("EmbeddedStatement", Statement.Null);

        // some pre-defined roles for most used punctuations
        public static readonly TokenRole LParenthesisToken = new TokenRole("(");
        public static readonly TokenRole RParenthesisToken = new TokenRole(")");
        public static readonly TokenRole LBracketToken = new TokenRole("[");
        public static readonly TokenRole RBracketToken = new TokenRole("]");
        public static readonly TokenRole LBraceToken = new TokenRole("{");
        public static readonly TokenRole RBraceToken = new TokenRole("}");
        public static readonly TokenRole LChevronToken = new TokenRole("<");
        public static readonly TokenRole RChevronToken = new TokenRole(">");
        public static readonly TokenRole CommaToken = new TokenRole(",");
        public static readonly TokenRole DotToken = new TokenRole(".");
        public static readonly TokenRole SemicolonToken = new TokenRole(";");
        public static readonly TokenRole AssignToken = new TokenRole("=");
        public static readonly TokenRole ColonToken = new TokenRole(":");
        public static readonly TokenRole RangeToken = new TokenRole("..");
        public static readonly TokenRole IncludeToken = new TokenRole("(-");
        public static readonly TokenRole ReturnToken = new TokenRole("->");
        public static readonly TokenRole IfToken = new TokenRole("if");
        public static readonly TokenRole ForToken = new TokenRole("for");
        public static readonly TokenRole InToken = new TokenRole("in");
        public static readonly Role<CommentNode> CommentRole = new Role<CommentNode>("Comment");

    }
}

