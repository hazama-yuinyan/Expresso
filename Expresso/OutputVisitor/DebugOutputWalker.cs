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

        void PrintList<TObject>(IEnumerable<TObject> list)
            where TObject : AstNode
        {
            int length = list.Count();
            var enumerator = list.GetEnumerator();
            for(int i = 0; i < length; ++i){
                if(i != 0)
                    writer.Write(", ");

                enumerator.MoveNext();
                TObject obj = enumerator.Current;
                obj.AcceptWalker(this);
            }
        }

        void PrintPairList<T>(IEnumerable<Tuple<T, T>> list, Func<T, T, string> connector)
            where T : AstNode
        {
            int length = list.Count();
            var enumerator = list.GetEnumerator();
            for(int i = 0; i < length; ++i){
                if(i != 0)
                    writer.Write(", ");

                enumerator.MoveNext();
                Tuple<T, T> pair = enumerator.Current;
                pair.Item1.AcceptWalker(this);
                writer.Write(connector(pair.Item1, pair.Item2));
                pair.Item2.AcceptWalker(this);
            }
        }

        void PrintPairList<T, U>(IEnumerable<Tuple<T, U>> list, string delimiter)
            where T : AstNode
            where U : AstNode
        {

        }

        #region IAstWalker implementation

        public void VisitAssignment(AssignmentExpression assignment)
        {
            assignment.Left.AcceptWalker(this);
            writer.Write(" = ");
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
            writer.Write(valueBindingForStmt.Modifiers);
            writer.Write(' ');
            foreach(var initializer in valueBindingForStmt.Variables){
                writer.Write(initializer.Name);
                writer.Write(" in ");
                initializer.Initializer.AcceptWalker(this);
            }
        }

        public void VisitIdentifier(Identifier ident)
        {
            if(ident.Type.IsNull){
                writer.Write(ident.Name);
            }else{
                writer.Write(ident.Name);
                writer.Write(" (- ");
                ident.Type.AcceptWalker(this);
                writer.Write(" @ ");
                if(ident.IdentifierId == 0)
                    writer.Write("<invalid>");
                else
                    writer.Write("<id: {0}>", ident.IdentifierId);
            }
        }

        public void VisitIfStatement(IfStatement ifStmt)
        {
            writer.Write("if ");
            ifStmt.Condition.AcceptWalker(this);
            writer.Write("{...}");
            if(!ifStmt.FalseBlock.IsNull)
                writer.Write("else{...}");
        }

        public void VisitMemberReference(MemberReference memRef)
        {
            memRef.Target.AcceptWalker(this);
            writer.Write(".");
            memRef.Member.AcceptWalker(this);
        }

        public void VisitAst(ExpressoAst ast)
        {
            writer.Write("<module ");
            writer.Write(ast.Name);
            writer.Write(" : ");
            writer.Write(ast.Declarations.Count);
            writer.Write(">");
        }

        public void VisitNewExpression(NewExpression newExpr)
        {
            writer.Write("new ");
            newExpr.CreationExpression.AcceptWalker(this);
        }

        public void VisitSequenceInitializer(SequenceInitializer seqInitializer)
        {
            var type = CSharpCompilerHelper.GetContainerType(seqInitializer.ObjectType);
            var enclosures = Enclosures[type];
            writer.Write(enclosures[0]);

            int i = 0;
            foreach(var item in seqInitializer.Items){
                if(i >= 5){
                    writer.Write(", ~~");
                    break;
                }else if(i != 0){
                    writer.Write(", ");
                }

                item.AcceptWalker(this);
                ++i;
            }

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

        public void VisitSequence(SequenceExpression seqExpr)
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

        public void VisitComprehensionExpression(ComprehensionExpression comp)
        {
            writer.Write("[");
            comp.Item.AcceptWalker(this);
            comp.Body.AcceptWalker(this);
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

        public void VisitIntgerSequenceExpression(IntegerSequenceExpression intSeq)
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
            int i = 0;
            foreach(var item in creation.Items){
                if(i > 5){
                    writer.Write(", ...");
                    break;
                }else if(i != 0){
                    writer.Write(", ");
                }

                item.AcceptWalker(this);
                ++i;
            }
            writer.Write("}");
        }

        public void VisitMatchClause(MatchPatternClause matchClause)
        {
            int i = 0;
            foreach(var pattern in matchClause.Patterns){
                if(i >= 3){
                    writer.Write("...");
                    break;
                }else if(i != 0){
                    writer.Write(" | ");
                }

                pattern.AcceptWalker(this);
                ++i;
            }

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
            writer.Write(simpleType.IdentifierNode);
            if(simpleType.TypeArguments.HasChildren){
                writer.Write("<");
                PrintList(simpleType.TypeArguments);
                writer.Write(">");
            }
        }

        public void VisitPrimitiveType(PrimitiveType primitiveType)
        {
            writer.Write(primitiveType.KnownTypeCode);
            //primitiveType.KeywordToken;
        }

        public void VisitReferenceType(ReferenceType referenceType)
        {
        }

        public void VisitMemberType(MemberType memberType)
        {
            memberType.Target.AcceptWalker(this);
            writer.Write("::");
            writer.Write(memberType.MemberName);
        }

        public void VisitFunctionType(FunctionType funcType)
        {
            writer.Write("|");
            PrintList(funcType.Parameters);
            writer.Write("| -> ");
            funcType.ReturnType.AcceptWalker(this);
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
            if(!importDecl.AliasNameToken.IsNull){
                writer.Write(importDecl.ModuleName);
                writer.Write(" as ");
                writer.Write(importDecl.AliasName);
            }else{
                writer.Write(importDecl.ModuleName);
            }
        }

        public void VisitFunctionDeclaration(FunctionDeclaration funcDecl)
        {
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
            writer.Write("{0} ", typeDecl.ClassType.ToString());
            writer.Write(typeDecl.Name);
            if(typeDecl.BaseTypes.HasChildren){
                writer.Write(" : ");
                PrintList(typeDecl.BaseTypes);
            }
            writer.Write("{...}");
        }

        public void VisitFieldDeclaration(FieldDeclaration fieldDecl)
        {
            if(fieldDecl.HasModifier(Modifiers.Immutable))
                writer.Write("let ");
            else
                writer.Write("var ");

            int i = 0;
            foreach(var init in fieldDecl.Initializers){
                if(i != 0){
                    writer.Write(", ...");
                    break;
                }

                init.AcceptWalker(this);
                ++i;
            }
        }

        public void VisitParameterDeclaration(ParameterDeclaration parameterDecl)
        {
            parameterDecl.NameToken.AcceptWalker(this);
            if(!parameterDecl.Option.IsNull){
                writer.Write(" = ");
                parameterDecl.Option.AcceptWalker(this);
            }
        }

        public void VisitVariableInitializer(VariableInitializer initializer)
        {
            initializer.NameToken.AcceptWalker(this);
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

        public void VisitValueBindingPattern(ValueBindingPattern valueBindingPattern)
        {
            if(valueBindingPattern.Modifiers.HasFlag(Modifiers.Immutable))
                writer.Write("let ");
            else
                writer.Write("var ");

            valueBindingPattern.Variables.AcceptWalker(this);
        }

        public void VisitCollectionPattern(CollectionPattern collectionPattern)
        {
            var type = CSharpCompilerHelper.GetNativeType(collectionPattern.CollectionType);
            if(type == typeof(Dictionary<,>)){
                writer.Write("{");
                int i = 0;
                foreach(var elem in collectionPattern.Items){
                    if(i > 5){
                        writer.Write(", ...");
                        break;
                    }else if(i != 0){
                        writer.Write(", ");
                    }

                    elem.AcceptWalker(this);
                    ++i;
                }
                writer.Write("}");
            }else if(type == typeof(List<>) || type == typeof(Array)){
                writer.Write("[");
                int i = 0;
                foreach(var elem in collectionPattern.Items){
                    if(i > 5){
                        writer.Write(", ...");
                        break;
                    }else if(i != 0){
                        writer.Write(", ");
                    }

                    elem.AcceptWalker(this);
                    ++i;
                }
                writer.Write("]");
            }
        }

        public void VisitDestructuringPattern(DestructuringPattern destructuringPattern)
        {
            destructuringPattern.TypePath.AcceptWalker(this);
            writer.Write("{");
            destructuringPattern.Items.AcceptWalker(this);
            writer.Write("}");
        }

        public void VisitTuplePattern(TuplePattern tuplePattern)
        {
            writer.Write("(");
            tuplePattern.Patterns.AcceptWalker(this);
            writer.Write(")");
        }

        public void VisitExpressionPattern(ExpressionPattern exprPattern)
        {
            exprPattern.Expression.AcceptWalker(this);
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

