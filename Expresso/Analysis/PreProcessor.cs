using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Expresso.CodeGen;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast.Analysis
{
    using RegexMatch = System.Text.RegularExpressions.Match;

    /// <summary>
    /// Manages the pre process phase.
    /// During the pre process phase, we will interpolate strings.
    /// </summary>
    class PreProcessor : IAstWalker
    {
        static Regex InterpolationTargetFinder = new Regex(@"\${([^}]+)}", RegexOptions.Compiled);
        Parser parser;

        PreProcessor(Parser parser)
        {
            this.parser = parser;
        }

        #region Public surface
        internal static void PerformPreProcess(ExpressoAst ast, Parser parser)
        {
            var processor = new PreProcessor(parser);
            processor.VisitAst(ast);
        }
        #endregion

        public void VisitAliasDeclaration(AliasDeclaration aliasDecl)
        {
            // no op
        }

        public void VisitAssignment(AssignmentExpression assignment)
        {
            assignment.Left.AcceptWalker(this);
            assignment.Right.AcceptWalker(this);
        }

        public void VisitAst(ExpressoAst ast)
        {
            ast.Imports.AcceptWalker(this);
            ast.Declarations.AcceptWalker(this);
        }

        public void VisitBinaryExpression(BinaryExpression binaryExpr)
        {
            binaryExpr.Left.AcceptWalker(this);
            binaryExpr.Right.AcceptWalker(this);
        }

        public void VisitBlock(BlockStatement block)
        {
            block.Statements.AcceptWalker(this);
        }

        public void VisitBreakStatement(BreakStatement breakStmt)
        {
            // no op
        }

        public void VisitCallExpression(CallExpression callExpr)
        {
            callExpr.Target.AcceptWalker(this);
            callExpr.Arguments.AcceptWalker(this);
        }

        public void VisitCastExpression(CastExpression castExpr)
        {
            castExpr.Target.AcceptWalker(this);
            castExpr.ToExpression.AcceptWalker(this);
        }

        public void VisitCatchClause(CatchClause catchClause)
        {
            VisitBlock(catchClause.Body);
        }

        public void VisitClosureLiteralExpression(ClosureLiteralExpression closure)
        {
            closure.Parameters.AcceptWalker(this);
            VisitBlock(closure.Body);
        }

        public void VisitCollectionPattern(CollectionPattern collectionPattern)
        {
            collectionPattern.Items.AcceptWalker(this);
        }

        public void VisitCommentNode(CommentNode comment)
        {
            // no op
        }

        public void VisitComprehensionExpression(ComprehensionExpression comp)
        {
            VisitComprehensionForClause(comp.Body);
        }

        public void VisitComprehensionForClause(ComprehensionForClause compFor)
        {
            compFor.Left.AcceptWalker(this);
            compFor.Target.AcceptWalker(this);
            compFor.Body.AcceptWalker(this);
        }

        public void VisitComprehensionIfClause(ComprehensionIfClause compIf)
        {
            compIf.Condition.AcceptWalker(this);
            compIf.Body.AcceptWalker(this);
        }

        public void VisitConditionalExpression(ConditionalExpression condExpr)
        {
            condExpr.Condition.AcceptWalker(this);
            condExpr.TrueExpression.AcceptWalker(this);
            condExpr.FalseExpression.AcceptWalker(this);
        }

        public void VisitContinueStatement(ContinueStatement continueStmt)
        {
            // no op
        }

        public void VisitDestructuringPattern(DestructuringPattern destructuringPattern)
        {
            destructuringPattern.TypePath.AcceptWalker(this);
            destructuringPattern.Items.AcceptWalker(this);
        }

        public void VisitDoWhileStatement(DoWhileStatement doWhileStmt)
        {
            VisitWhileStatement(doWhileStmt.Delegator);
        }

        public void VisitEmptyStatement(EmptyStatement emptyStmt)
        {
            // no op
        }

        public void VisitExpressionPattern(ExpressionPattern exprPattern)
        {
            exprPattern.Expression.AcceptWalker(this);
        }

        public void VisitExpressionStatement(ExpressionStatement exprStmt)
        {
            exprStmt.Expression.AcceptWalker(this);
        }

        public void VisitExpressoTokenNode(ExpressoTokenNode tokenNode)
        {
            // no op
        }

        public void VisitFieldDeclaration(FieldDeclaration fieldDecl)
        {
            fieldDecl.Initializers.AcceptWalker(this);
        }

        public void VisitFinallyClause(FinallyClause finallyClause)
        {
            VisitBlock(finallyClause.Body);
        }

        public void VisitForStatement(ForStatement forStmt)
        {
            forStmt.Left.AcceptWalker(this);
            forStmt.Target.AcceptWalker(this);
            VisitBlock(forStmt.Body);
        }

        public void VisitFunctionDeclaration(FunctionDeclaration funcDecl)
        {
            funcDecl.Parameters.AcceptWalker(this);
            funcDecl.ReturnType.AcceptWalker(this);
            VisitBlock(funcDecl.Body);
        }

        public void VisitFunctionType(FunctionType funcType)
        {
            funcType.ReturnType.AcceptWalker(this);
            funcType.Parameters.AcceptWalker(this);
        }

        public void VisitIdentifier(Identifier ident)
        {
            // no op
        }

        public void VisitIdentifierPattern(IdentifierPattern identifierPattern)
        {
            identifierPattern.InnerPattern.AcceptWalker(this);
        }

        public void VisitIfStatement(IfStatement ifStmt)
        {
            ifStmt.Condition.AcceptWalker(this);
            VisitBlock(ifStmt.TrueBlock);
            VisitBlock(ifStmt.FalseBlock);
        }

        public void VisitIgnoringRestPattern(IgnoringRestPattern restPattern)
        {
            // no op
        }

        public void VisitImportDeclaration(ImportDeclaration importDecl)
        {
            // Here's the good place to import names from other files
            // All external names will be imported into the module scope we are currently compiling
            if(!importDecl.TargetFile.IsNull){
                Parser inner_parser = null;
                if(importDecl.TargetFilePath.EndsWith(".exs", StringComparison.CurrentCulture)){
                    inner_parser = new Parser(parser.scanner.OpenChildFile(importDecl.TargetFilePath));
                    inner_parser.Parse();

                    PerformPreProcess(inner_parser.TopmostAst, inner_parser);
                    ExpressoNameBinder.BindAst(inner_parser.TopmostAst, inner_parser);

                    parser.InnerParsers.Add(inner_parser);
                }

                var first_import_path = importDecl.ImportPaths.First();
                var table = importDecl.TargetFilePath.EndsWith(".dll", StringComparison.CurrentCulture) ? ExpressoCompilerHelpers.GetSymbolTableForAssembly(parser.scanner.GetFullPath(importDecl.TargetFilePath))
                                      : inner_parser.Symbols;
                if(!first_import_path.Name.Contains("::") && !first_import_path.Name.Contains("."))
                    parser.Symbols.AddExternalSymbols(table, importDecl.AliasTokens.First().Name);
                else
                    parser.Symbols.AddExternalSymbols(table, importDecl.ImportPaths, importDecl.AliasTokens);

                if(importDecl.TargetFilePath.EndsWith(".exs", StringComparison.CurrentCulture)){
                    if(!first_import_path.Name.Contains("::") && !first_import_path.Name.Contains(".")){
                        ((ExpressoAst)importDecl.Parent).ExternalModules.Add(inner_parser.TopmostAst);
                    }else{
                        var filtered = FilterAst(inner_parser.TopmostAst, importDecl.ImportPaths);
                        ((ExpressoAst)importDecl.Parent).ExternalModules.Add(filtered);
                    }
                }
            }

            // Make the module name type-aware
            foreach(var pair in importDecl.ImportPaths.Zip(importDecl.AliasTokens, (l, r) => new Tuple<Identifier, Identifier>(l, r))){
                var type = AstType.MakeSimpleType(pair.Item1.Name);
                pair.Item1.Type = type;
                // These 2 statements are needed because otherwise aliases won't get IdentifierIds
                UniqueIdGenerator.DefineNewId(pair.Item1);

                // Types from the standard library and external dlls should know the real name
                // in order for the compiler to keep track of them.
                if(type.Name.StartsWith("System.", StringComparison.CurrentCulture) || importDecl.TargetFilePath.EndsWith(".dll", StringComparison.CurrentCulture))
                    pair.Item2.Type = type.Clone();

                // These 2 statements are needed because otherwise aliases won't get IdentifierIds
                UniqueIdGenerator.DefineNewId(pair.Item2);
            }
        }

        public void VisitIndexerExpression(IndexerExpression indexExpr)
        {
            indexExpr.Target.AcceptWalker(this);
            indexExpr.Arguments.AcceptWalker(this);
        }

        public void VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq)
        {
            intSeq.Start.AcceptWalker(this);
            intSeq.End.AcceptWalker(this);
            intSeq.Step.AcceptWalker(this);
        }

        public void VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
        {
            keyValue.KeyExpression.AcceptWalker(this);
            keyValue.ValueExpression.AcceptWalker(this);
        }

        public void VisitKeyValuePattern(KeyValuePattern keyValuePattern)
        {
            keyValuePattern.Value.AcceptWalker(this);
        }

        public void VisitLiteralExpression(LiteralExpression literal)
        {
            if(literal.Type.Name == "string"){
                var template = (string)literal.Value;
                var matches = InterpolationTargetFinder.Matches(template);

                var exprs = new List<Expression>();
                foreach(RegexMatch match in matches){
                    var groups = match.Groups;
                    var inner_parser = new Parser(new Scanner(new MemoryStream(Encoding.UTF8.GetBytes(groups[1].Value))));
                    try{
                        exprs.Add(inner_parser.ParseExpression());
                    }
                    catch(ParserException e){
                        parser.ReportSemanticError(
                            "Error ES3000: A string interpolation error: {0}",
                            literal,
                            e.Message
                        );
                    }
                }

                int counter = 0;
                var replaced_string = InterpolationTargetFinder.Replace(template, match => "{" + counter++.ToString() + "}");
                replaced_string = replaced_string.Replace("$$", "$");
                literal.SetValue(replaced_string, replaced_string);

                if(matches.Count > 0){
                    var call_expr = Expression.MakeCallExpr(
                        Expression.MakeMemRef(
                            Expression.MakePath(
                                AstNode.MakeIdentifier("string", AstType.MakePlaceholderType())
                            ),
                            AstNode.MakeIdentifier("format", AstType.MakePlaceholderType())
                        ),
                        new []{literal.Clone()}.Concat(exprs),
                        literal.StartLocation
                    );
                    literal.ReplaceWith(call_expr);
                }
            }
        }

        public void VisitMatchClause(MatchPatternClause matchClause)
        {
            matchClause.Patterns.AcceptWalker(this);
            matchClause.Body.AcceptWalker(this);
        }

        public void VisitMatchStatement(MatchStatement matchStmt)
        {
            matchStmt.Target.AcceptWalker(this);
            matchStmt.Clauses.AcceptWalker(this);
        }

        public void VisitMemberReference(MemberReferenceExpression memRef)
        {
            memRef.Target.AcceptWalker(this);
        }

        public void VisitMemberType(MemberType memberType)
        {
            memberType.Target.AcceptWalker(this);
        }

        public void VisitNewLine(NewLineNode newlineNode)
        {
            // no op
        }

        public void VisitNullNode(AstNode nullNode)
        {
            // no op
        }

        public void VisitNullReferenceExpression(NullReferenceExpression nullRef)
        {
            // no op
        }

        public void VisitObjectCreationExpression(ObjectCreationExpression creation)
        {
            creation.TypePath.AcceptWalker(this);
            creation.Items.AcceptWalker(this);
        }

        public void VisitParameterDeclaration(ParameterDeclaration parameterDecl)
        {
            parameterDecl.Option.AcceptWalker(this);
        }

        public void VisitParameterType(ParameterType paramType)
        {
            // no op
        }

        public void VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
        {
            parensExpr.Expression.AcceptWalker(this);
        }

        public void VisitPathExpression(PathExpression pathExpr)
        {
            pathExpr.Items.AcceptWalker(this);
        }

        public void VisitPatternPlaceholder(AstNode placeholder, Pattern child)
        {
            // no op
        }

        public void VisitPatternWithType(PatternWithType pattern)
        {
            pattern.Pattern.AcceptWalker(this);
            pattern.Type.AcceptWalker(this);
        }

        public void VisitPlaceholderType(PlaceholderType placeholderType)
        {
            // no op
        }

        public void VisitPrimitiveType(PrimitiveType primitiveType)
        {
            // no op
        }

        public void VisitReferenceType(ReferenceType referenceType)
        {
            // no op
        }

        public void VisitReturnStatement(ReturnStatement returnStmt)
        {
            returnStmt.Expression.AcceptWalker(this);
        }

        public void VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
        {
            // no op
        }

        public void VisitSequenceExpression(SequenceExpression seqExpr)
        {
            seqExpr.Items.AcceptWalker(this);
        }

        public void VisitSequenceInitializer(SequenceInitializer seqInitializer)
        {
            VisitSimpleType(seqInitializer.ObjectType);
            seqInitializer.Items.AcceptWalker(this);
        }

        public void VisitSimpleType(SimpleType simpleType)
        {
            simpleType.TypeArguments.AcceptWalker(this);
        }

        public void VisitSuperReferenceExpression(SuperReferenceExpression superRef)
        {
            // no op
        }

        public void VisitTextNode(TextNode textNode)
        {
            // no op
        }

        public void VisitThrowStatement(ThrowStatement throwStmt)
        {
            VisitObjectCreationExpression(throwStmt.CreationExpression);
        }

        public void VisitTryStatement(TryStatement tryStmt)
        {
            VisitBlock(tryStmt.EnclosingBlock);
            tryStmt.CatchClauses.AcceptWalker(this);
        }

        public void VisitTuplePattern(TuplePattern tuplePattern)
        {
            tuplePattern.Patterns.AcceptWalker(this);
        }

        public void VisitTypeDeclaration(TypeDeclaration typeDecl)
        {
            typeDecl.BaseTypes.AcceptWalker(this);
            typeDecl.Members.AcceptWalker(this);
        }

        public void VisitUnaryExpression(UnaryExpression unaryExpr)
        {
            unaryExpr.Operand.AcceptWalker(this);
        }

        public void VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStmt)
        {
            valueBindingForStmt.Initializer.AcceptWalker(this);
            VisitBlock(valueBindingForStmt.Body);
        }

        public void VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
        {
            varDecl.Variables.AcceptWalker(this);
        }

        public void VisitVariableInitializer(VariableInitializer initializer)
        {
            initializer.Initializer.AcceptWalker(this);
        }

        public void VisitWhileStatement(WhileStatement whileStmt)
        {
            whileStmt.Condition.AcceptWalker(this);
            VisitBlock(whileStmt.Body);
        }

        public void VisitWhitespace(WhitespaceNode whitespaceNode)
        {
            // no op
        }

        public void VisitWildcardPattern(WildcardPattern wildcardPattern)
        {
            // no op
        }

        public void VisitYieldStatement(YieldStatement yieldStmt)
        {
            yieldStmt.Expression.AcceptWalker(this);
        }

        static ExpressoAst FilterAst(ExpressoAst ast, IEnumerable<Identifier> importPaths)
        {
            foreach(var decl in ast.Declarations){
                if(decl is FieldDeclaration field){
                    bool found = false;
                    foreach(var init in field.Initializers){
                        if(IsExportedItem(init.Name, importPaths))
                            found = true;
                    }

                    if(!found)
                        decl.Remove();
                }else{
                    var decl_name = decl.Name;
                    if(!IsExportedItem(decl_name, importPaths))
                        decl.Remove();
                }
            }

            return ast;
        }

        static bool IsExportedItem(string name, IEnumerable<Identifier> importPaths)
        {
            return importPaths.Any(ident => {
                var tmp = ident.Name.Substring(ident.Name.LastIndexOf("::", StringComparison.CurrentCulture) + "..".Length);
                var target_name = tmp.Substring(tmp.LastIndexOf(".", StringComparison.CurrentCulture) + ".".Length);
                return target_name == name;
            });
        }
    }
}
