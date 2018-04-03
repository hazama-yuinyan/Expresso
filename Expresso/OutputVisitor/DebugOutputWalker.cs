using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Expresso.CodeGen;


namespace Expresso.Ast
{
    /// <summary>
    /// An ast walker that outputs each node as a sequence of strings for debugging purpose.
    /// </summary>
    public class DebugOutputWalker : IAstWalker
    {
        static Dictionary<Type, string[]> Enclosures = new Dictionary<Type, string[]>{
            {typeof(List<>), new []{"[", "]"}},
            {typeof(Array), new []{"[", "]"}},
            {typeof(Dictionary<,>), new []{"{", "}"}},
            {typeof(Tuple), new []{"(", ")"}}
        };

        readonly TextWriter writer;

        public DebugOutputWalker(TextWriter writer)
        {
            this.writer = writer;
        }

        void PrintLongList<TObject>(IEnumerable<TObject> list, string terminator = "...", int numberItems = 5)
            where TObject : AstNode
        {
            int i = 0, max_items = numberItems + 1;
            foreach(var elem in list){
                if(i > max_items){
                    writer.Write(", {0}", terminator);
                    break;
                }else if(i != 0){
                    writer.Write(", ");
                }

                elem.AcceptWalker(this);
                ++i;
            }
        }

        void PrintList<T>(IEnumerable<T> list)
            where T : AstNode
        {
            bool first = true;
            foreach(var elem in list){
                if(first)
                    first = false;
                else
                    writer.Write(", ");

                elem.AcceptWalker(this);
            }
        }

        #region IAstWalker implementation

        public void VisitAssignment(AssignmentExpression assignment)
        {
            assignment.Left.AcceptWalker(this);
            writer.Write(" ");
            switch(assignment.Operator){
                case OperatorType.Assign:
                writer.Write("= ");
                break;

                case OperatorType.Plus:
                writer.Write("+= ");
                break;

                case OperatorType.Minus:
                writer.Write("-= ");
                break;

                case OperatorType.Times:
                writer.Write("*= ");
                break;

                case OperatorType.Divide:
                writer.Write("/= ");
                break;

                case OperatorType.Modulus:
                writer.Write("%= ");
                break;

                case OperatorType.Power:
                writer.Write("**= ");
                break;
            }
            assignment.Right.AcceptWalker(this);
        }

        public void VisitBinaryExpression(BinaryExpression binaryExpr)
        {
            binaryExpr.Left.AcceptWalker(this);
            writer.Write(' ');
            binaryExpr.OperatorToken.AcceptWalker(this);
            writer.Write(' ');
            binaryExpr.Right.AcceptWalker(this);
        }

        public void VisitBlock(BlockStatement block)
        {
            writer.Write("{count: ");
            writer.Write(block.Statements.Count);
            writer.Write("...}");
        }

        public void VisitCallExpression(CallExpression callExpr)
        {
            callExpr.Target.AcceptWalker(this);
            writer.Write("(");
            PrintList(callExpr.Arguments);
            writer.Write(")");
        }

        public void VisitCastExpression(CastExpression castExpr)
        {
            castExpr.Target.AcceptWalker(this);
            writer.Write(" as ");
            castExpr.ToExpression.AcceptWalker(this);
        }

        public void VisitCatchClause(CatchClause catchClause)
        {
            writer.Write("catch ");
            VisitIdentifier(catchClause.Identifier);
            writer.Write("{...}");
        }

        public void VisitConstant(LiteralExpression literal)
        {
            var value_type = literal.Value.GetType();
            string enclosure = null;
            switch(value_type.Name.ToLower()){
            case "string":
                enclosure = "\"";
                break;

            case "char":
                enclosure = "'";
                break;

            default:
                enclosure = "";
                break;
            }

            writer.Write("{0}{1}{2}", enclosure, literal.Value, enclosure);
        }

        public void VisitBreakStatement(BreakStatement breakStmt)
        {
            writer.Write("break upto ");
            breakStmt.Count.AcceptWalker(this);
        }

        public void VisitContinueStatement(ContinueStatement continueStmt)
        {
            writer.Write("continue upto ");
            continueStmt.Count.AcceptWalker(this);
        }

        public void VisitDoWhileStatement(DoWhileStatement doWhileStmt)
        {
            writer.Write("do{");
            writer.Write("...{0}", doWhileStmt.Delegator.Body.Statements.Count);
            writer.Write("}while ");
            doWhileStmt.Delegator.Condition.AcceptWalker(this);
        }

        public void VisitEmptyStatement(EmptyStatement emptyStmt)
        {
            writer.Write("<empty>");
        }

        public void VisitExpressionStatement(ExpressionStatement exprStmt)
        {
            exprStmt.Expression.AcceptWalker(this);
        }

        public void VisitForStatement(ForStatement forStmt)
        {
            writer.Write("for ");
            forStmt.Left.AcceptWalker(this);
            writer.Write(" in ");
            forStmt.Target.AcceptWalker(this);
            writer.Write("{...}");
        }

        public void VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStmt)
        {
            writer.Write("for ");
            writer.Write(valueBindingForStmt.Modifiers.HasFlag(Modifiers.Immutable) ? "let" : "var");
            writer.Write(' ');
            valueBindingForStmt.Initializer.Pattern.AcceptWalker(this);
            writer.Write(" in ");
            valueBindingForStmt.Initializer.Initializer.AcceptWalker(this);
        }

        public void VisitIdentifier(Identifier ident)
        {
            if(ident.Modifiers.HasFlag(Modifiers.Immutable))
                writer.Write("Immutable ");
            
            writer.Write(ident.Name);
            if(!ident.Type.IsNull){
                writer.Write(" (- ");
                ident.Type.AcceptWalker(this);
            }
            writer.Write(" @ ");
            if(ident.IdentifierId == 0)
                writer.Write("<invalid>");
            else
                writer.Write("<id: {0}>", ident.IdentifierId);
        }

        public void VisitIfStatement(IfStatement ifStmt)
        {
            writer.Write("if ");
            ifStmt.Condition.AcceptWalker(this);
            writer.Write("{...}");
            if(!ifStmt.FalseBlock.IsNull)
                writer.Write("else{...}");
        }

        public void VisitMemberReference(MemberReferenceExpression memRef)
        {
            memRef.Target.AcceptWalker(this);
            writer.Write(".");
            VisitIdentifier(memRef.Member);
        }

        public void VisitAst(ExpressoAst ast)
        {
            writer.Write("<module ");
            writer.Write(ast.Name);
            writer.Write(" : ");
            writer.Write(ast.Declarations.Count);
            writer.Write(">");
        }

        public void VisitSequenceInitializer(SequenceInitializer seqInitializer)
        {
            var type = CSharpCompilerHelper.GetContainerType(seqInitializer.ObjectType);
            var enclosures = Enclosures[type];
            writer.Write(enclosures[0]);

            PrintLongList(seqInitializer.Items, "~~");

            if(seqInitializer.ObjectType.Identifier == "vector")
                writer.Write("...");

            writer.Write(enclosures[1]);
        }

        public void VisitReturnStatement(ReturnStatement returnStmt)
        {
            writer.Write("return ");
            if(!returnStmt.Expression.IsNull)
                returnStmt.Expression.AcceptWalker(this);

            writer.Write(";");
        }

        public void VisitMatchStatement(MatchStatement matchStmt)
        {
            writer.Write("match ");
            matchStmt.Target.AcceptWalker(this);
            writer.Write(" {");
            writer.Write(matchStmt.Clauses.Count);
            writer.Write("...}");
        }

        public void VisitThrowStatement(ThrowStatement throwStmt)
        {
            writer.Write("throw ");
            throwStmt.CreationExpression.AcceptWalker(this);
        }

        public void VisitTryStatement(TryStatement tryStmt)
        {
            writer.Write("try");
            VisitBlock(tryStmt.EnclosingBlock);
            tryStmt.CatchClauses.AcceptWalker(this);
            tryStmt.FinallyClause.AcceptWalker(this);
        }

        public void VisitMatchPatternClause(MatchPatternClause clause)
        {
            bool first = true;
            foreach(var pattern in clause.Patterns){
                if(!first)
                    writer.Write(" | ");
                else
                    first = false;

                pattern.AcceptWalker(this);
            }

            writer.Write(" => ");
            clause.Body.AcceptWalker(this);
        }

        public void VisitSequenceExpression(SequenceExpression seqExpr)
        {
            PrintList(seqExpr.Items);
        }

        public void VisitUnaryExpression(UnaryExpression unaryExpr)
        {
            unaryExpr.OperatorToken.AcceptWalker(this);
            unaryExpr.Operand.AcceptWalker(this);
        }

        public void VisitWhileStatement(WhileStatement whileStmt)
        {
            writer.Write("while ");
            whileStmt.Condition.AcceptWalker(this);
            writer.Write("{...}");
        }

        public void VisitYieldStatement(YieldStatement yieldStmt)
        {
            writer.Write("yield ");
            yieldStmt.Expression.AcceptWalker(this);
            writer.Write(";");
        }

        public void VisitNullNode(AstNode nullNode)
        {
            writer.Write("<null>");
        }

        public void VisitPatternPlaceholder(AstNode placeholder, ICSharpCode.NRefactory.PatternMatching.Pattern child)
        {
            writer.Write("<placeholder for patterns>");
        }

        public void VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
        {
            if(varDecl.Modifiers.HasFlag(Modifiers.Immutable))
                writer.Write("let ");
            else
                writer.Write("var ");

            int i = 0;
            foreach(var variable in varDecl.Variables){
                if(i != 0){
                    writer.Write(", ...;");
                    break;
                }

                variable.AcceptWalker(this);
                ++i;
            }
        }

        public void VisitClosureLiteralExpression(ClosureLiteralExpression closure)
        {
            writer.Write("|");
            PrintList(closure.Parameters);
            writer.Write("| -> ");
            closure.ReturnType.AcceptWalker(this);
            VisitBlock(closure.Body);
        }

        public void VisitComprehensionExpression(ComprehensionExpression comp)
        {
            writer.Write("[");
            comp.Item.AcceptWalker(this);
            VisitComprehensionForClause(comp.Body);
            writer.Write("]");
        }

        public void VisitComprehensionForClause(ComprehensionForClause compFor)
        {
            writer.Write("... for ");
            compFor.Left.AcceptWalker(this);
            writer.Write(" in ");
            compFor.Target.AcceptWalker(this);
            if(!compFor.Body.IsNull)
                writer.Write(" ...");
        }

        public void VisitComprehensionIfClause(ComprehensionIfClause compIf)
        {
            writer.Write("... if ");
            compIf.Condition.AcceptWalker(this);
            if(!compIf.Body.IsNull)
                writer.Write(" ...");
        }

        public void VisitConditionalExpression(ConditionalExpression condExpr)
        {
            condExpr.Condition.AcceptWalker(this);
            writer.Write(" ? ");
            condExpr.TrueExpression.AcceptWalker(this);
            writer.Write(" : ");
            condExpr.FalseExpression.AcceptWalker(this);
        }

        public void VisitFinallyClause(FinallyClause finallyClause)
        {
            writer.Write("finally");
            VisitBlock(finallyClause.Body);
        }

        public void VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
        {
            keyValue.KeyExpression.AcceptWalker(this);
            writer.Write(" : ");
            keyValue.ValueExpression.AcceptWalker(this);
        }

        public void VisitLiteralExpression(LiteralExpression literal)
        {
            writer.Write(literal.LiteralValue);
        }

        public void VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq)
        {
            intSeq.Lower.AcceptWalker(this);
            writer.Write(intSeq.UpperInclusive ? "..." : "..");
            intSeq.Upper.AcceptWalker(this);
            writer.Write(":");
            intSeq.Step.AcceptWalker(this);
        }

        public void VisitIndexerExpression(IndexerExpression indexExpr)
        {
            indexExpr.Target.AcceptWalker(this);
            writer.Write("[");
            PrintList(indexExpr.Arguments);
            writer.Write("]");
        }

        public void VisitPathExpression(PathExpression pathExpr)
        {
            bool first = true;
            foreach(var item in pathExpr.Items){
                if(!first)
                    writer.Write("::");
                else
                    first = false;

                item.AcceptWalker(this);
            }
        }

        public void VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
        {
            writer.Write("(");
            parensExpr.Expression.AcceptWalker(this);
            writer.Write(")");
        }

        public void VisitObjectCreationExpression(ObjectCreationExpression creation)
        {
            creation.TypePath.AcceptWalker(this);
            writer.Write("{");
            PrintLongList(creation.Items);
            writer.Write("}");
        }

        public void VisitMatchClause(MatchPatternClause matchClause)
        {
            PrintLongList(matchClause.Patterns, "...", 3);

            writer.Write(" => {...}");
        }

        public void VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
        {
            writer.Write("self");
        }

        public void VisitSuperReferenceExpression(SuperReferenceExpression superRef)
        {
            writer.Write("super");
        }

        public void VisitNullReferenceExpression(NullReferenceExpression nullRef)
        {
            writer.Write("null");
        }

        public void VisitCommentNode(CommentNode comment)
        {
            writer.Write("//" + comment.Text);
        }

        public void VisitTextNode(TextNode textNode)
        {
            writer.Write("<?");
            writer.Write(textNode.Text);
            writer.Write("?>");
        }

        public void VisitSimpleType(SimpleType simpleType)
        {
            VisitIdentifier(simpleType.IdentifierNode);
            if(simpleType.TypeArguments.Count > 0){
                writer.Write("<");
                PrintList(simpleType.TypeArguments);
                writer.Write(">");
            }
        }

        public void VisitPrimitiveType(PrimitiveType primitiveType)
        {
            writer.Write(primitiveType.KnownTypeCode);
        }

        public void VisitReferenceType(ReferenceType referenceType)
        {
            writer.Write("&");
            referenceType.BaseType.AcceptWalker(this);
        }

        public void VisitMemberType(MemberType memberType)
        {
            memberType.Target.AcceptWalker(this);
            writer.Write("::");
            VisitIdentifier(memberType.IdentifierNode);
        }

        public void VisitFunctionType(FunctionType funcType)
        {
            writer.Write("|");
            PrintList(funcType.Parameters);
            writer.Write("| -> ");
            funcType.ReturnType.AcceptWalker(this);
        }

        public void VisitParameterType(ParameterType paramType)
        {
            writer.Write("`");
            writer.Write(paramType.Name);
        }

        public void VisitPlaceholderType(PlaceholderType placeholderType)
        {
            writer.Write("<placeholder type>");
        }

        public void VisitAliasDeclaration(AliasDeclaration aliasDecl)
        {
            writer.Write("alias ");
            aliasDecl.Path.AcceptWalker(this);
            writer.Write(" as ");
            writer.Write(aliasDecl.AliasName);
        }

        public void VisitImportDeclaration(ImportDeclaration importDecl)
        {
            writer.Write("import ");
            if(importDecl.ImportPaths.Count > 1)
                writer.Write('{');

            PrintList(importDecl.ImportPaths);

            if(importDecl.ImportPaths.Count > 1)
                writer.Write('}');

            if(!importDecl.TargetFile.IsNull){
                writer.Write(" from ");
                writer.Write(importDecl.TargetFilePath);
            }

            writer.Write(" as ");
            if(importDecl.AliasTokens.Count > 1)
                writer.Write('{');
            
            PrintList(importDecl.AliasTokens);
            if(importDecl.AliasTokens.Count > 1)
                writer.Write('}');
        }

        public void VisitFunctionDeclaration(FunctionDeclaration funcDecl)
        {
            if(!funcDecl.Modifiers.HasFlag(Modifiers.None)){
                writer.Write(funcDecl.Modifiers);
                writer.Write(" ");
            }
            writer.Write("def ");
            writer.Write(funcDecl.Name);
            writer.Write("(");
            PrintList(funcDecl.Parameters);
            writer.Write(") -> ");
            funcDecl.ReturnType.AcceptWalker(this);
            writer.Write("{...}");
        }

        public void VisitTypeDeclaration(TypeDeclaration typeDecl)
        {
            writer.Write("{0} ", typeDecl.TypeKind.ToString());
            writer.Write(typeDecl.Name);
            if(typeDecl.BaseTypes.HasChildren){
                writer.Write(" : ");
                PrintList(typeDecl.BaseTypes);
            }
            writer.Write("{...}");
        }

        public void VisitFieldDeclaration(FieldDeclaration fieldDecl)
        {
            if(fieldDecl.HasModifier(Modifiers.Public))
                writer.Write("public ");
            else if(fieldDecl.HasModifier(Modifiers.Protected))
                writer.Write("protected ");
            else if(fieldDecl.HasModifier(Modifiers.Private))
                writer.Write("private ");
            
            if(fieldDecl.HasModifier(Modifiers.Immutable))
                writer.Write("let ");
            else
                writer.Write("var ");

            PrintLongList(fieldDecl.Initializers, "...", 2);
        }

        public void VisitParameterDeclaration(ParameterDeclaration parameterDecl)
        {
            VisitIdentifier(parameterDecl.NameToken);
            if(parameterDecl.IsVariadic)
                writer.Write("...");
            
            if(!parameterDecl.Option.IsNull){
                writer.Write(" = ");
                parameterDecl.Option.AcceptWalker(this);
            }
        }

        public void VisitVariableInitializer(VariableInitializer initializer)
        {
            initializer.Pattern.AcceptWalker(this);
            writer.Write(" = ");
            initializer.Initializer.AcceptWalker(this);
        }

        public void VisitWildcardPattern(WildcardPattern wildcardPattern)
        {
            writer.Write("_");
        }

        public void VisitIdentifierPattern(IdentifierPattern identifierPattern)
        {
            identifierPattern.Identifier.AcceptWalker(this);
            if(!identifierPattern.InnerPattern.IsNull){
                writer.Write(" @ ");
                identifierPattern.InnerPattern.AcceptWalker(this);
            }
        }

        public void VisitCollectionPattern(CollectionPattern collectionPattern)
        {
            var type = collectionPattern.CollectionType;
            if(type.Identifier == "dictionary"){
                writer.Write("{");
                PrintLongList(collectionPattern.Items);
                writer.Write("}");
            }else if(type.Identifier == "vector" || type.Identifier == "array"){
                writer.Write("[");
                PrintLongList(collectionPattern.Items);
                writer.Write("]");
            }
        }

        public void VisitDestructuringPattern(DestructuringPattern destructuringPattern)
        {
            destructuringPattern.TypePath.AcceptWalker(this);
            writer.Write("{");
            PrintLongList(destructuringPattern.Items);
            writer.Write("}");
        }

        public void VisitTuplePattern(TuplePattern tuplePattern)
        {
            writer.Write("(");
            PrintLongList(tuplePattern.Patterns);
            writer.Write(")");
        }

        public void VisitExpressionPattern(ExpressionPattern exprPattern)
        {
            exprPattern.Expression.AcceptWalker(this);
        }

        public void VisitIgnoringRestPattern(IgnoringRestPattern restPattern)
        {
            writer.Write("..");
        }

        public void VisitKeyValuePattern(KeyValuePattern keyValuePattern)
        {
            VisitIdentifier(keyValuePattern.KeyIdentifier);
            writer.Write(": ");
            keyValuePattern.Value.AcceptWalker(this);
        }

        public void VisitPatternWithType(PatternWithType pattern)
        {
            pattern.Pattern.AcceptWalker(this);
            writer.Write(" (- ");
            pattern.Type.AcceptWalker(this);
        }

        public void VisitNewLine(NewLineNode newlineNode)
        {
            writer.Write("\n");
        }

        public void VisitWhitespace(WhitespaceNode whitespaceNode)
        {
            writer.Write(whitespaceNode.WhitespaceText);
        }

        public void VisitExpressoTokenNode(ExpressoTokenNode tokenNode)
        {
            writer.Write(tokenNode.Token);
        }

        #endregion
    }
}

