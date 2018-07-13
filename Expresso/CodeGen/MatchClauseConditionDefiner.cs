using System;
using ICSharpCode.NRefactory.PatternMatching;
using Expresso.Ast;
using System.Reflection.Emit;

namespace Expresso.CodeGen
{
    public partial class CodeGenerator : IAstWalker<CSharpEmitterContext, Type>
    {
        /// <summary>
        /// This class is responsible for defining the conditions of match clauses before inspecting the body statements of a match clause.
        /// </summary>
        public class MatchClauseConditionDefiner : IAstWalker
	    {
            CodeGenerator generator;
            CSharpEmitterContext context;

            public MatchClauseConditionDefiner(CodeGenerator generator, CSharpEmitterContext context)
            {
                this.generator = generator;
                this.context = context;
            }

            public void VisitAliasDeclaration(AliasDeclaration aliasDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitAssignment(AssignmentExpression assignment)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitAst(ExpressoAst ast)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitAttributeSection(AttributeSection section)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitBinaryExpression(BinaryExpression binaryExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitBlock(BlockStatement block)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitBreakStatement(BreakStatement breakStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitCallExpression(CallExpression callExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitCastExpression(CastExpression castExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitCatchClause(CatchClause catchClause)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitClosureLiteralExpression(ClosureLiteralExpression closure)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitCollectionPattern(CollectionPattern collectionPattern)
            {
                collectionPattern.Items.AcceptWalker(this);
            }

            public void VisitCommentNode(CommentNode comment)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitComprehensionExpression(ComprehensionExpression comp)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitComprehensionForClause(ComprehensionForClause compFor)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitComprehensionIfClause(ComprehensionIfClause compIf)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitConditionalExpression(ConditionalExpression condExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitContinueStatement(ContinueStatement continueStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitDestructuringPattern(DestructuringPattern destructuringPattern)
            {
                destructuringPattern.TypePath.AcceptWalker(generator, context);

                if(destructuringPattern.IsEnum){
                    var variant_name = ((MemberType)destructuringPattern.TypePath).ChildType.Name;
                    var field = context.TargetType.GetField(HiddenMemberPrefix + variant_name);

                    generator.EmitLoadLocal(context.TemporaryVariable, false);
                    generator.EmitLoadField(field);
                    generator.il_generator.Emit(OpCodes.Ldnull);
                    generator.EmitBinaryOp(OperatorType.InEquality);
                }else{
                    var native_type = CSharpCompilerHelpers.GetNativeType(destructuringPattern.TypePath);

                    generator.EmitLoadLocal(context.TemporaryVariable, false);
                    generator.il_generator.Emit(OpCodes.Isinst, native_type);
                    generator.il_generator.Emit(OpCodes.Ldnull);
                    generator.EmitBinaryOp(OperatorType.InEquality);
                }
            }

            public void VisitDoWhileStatement(DoWhileStatement doWhileStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitEmptyStatement(EmptyStatement emptyStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitExpressionPattern(ExpressionPattern exprPattern)
            {
                // Common scinario in an expression pattern:
                // An integer sequence expression or a literal expression.
                // In the former case we should test an integer against an IntSeq type object using an IntSeq's method
                // while in the latter case we should just test the value against the literal
                context.RequestMethod = true;
                context.Method = null;
                var type = exprPattern.Expression.AcceptWalker(generator, context);
                context.RequestMethod = false;

                if(context.Method != null && context.Method.DeclaringType.Name == "ExpressoIntegerSequence"){
                    var method = context.Method;
                    context.Method = null;

                    generator.EmitLoadLocal(context.TemporaryVariable, false);
                    generator.il_generator.Emit(OpCodes.Callvirt, method);
                }else if(context.ContextAst is MatchStatement){
                    generator.EmitLoadLocal(context.TemporaryVariable, false);
                    generator.il_generator.Emit(OpCodes.Ceq);
                }
            }

            public void VisitExpressionStatement(ExpressionStatement exprStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitExpressoTokenNode(ExpressoTokenNode tokenNode)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitFieldDeclaration(FieldDeclaration fieldDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitFinallyClause(FinallyClause finallyClause)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitForStatement(ForStatement forStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitFunctionDeclaration(FunctionDeclaration funcDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitFunctionType(FunctionType funcType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitIdentifier(Identifier ident)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitIdentifierPattern(IdentifierPattern identifierPattern)
            {
                var symbol = generator.GetRuntimeSymbol(identifierPattern.Identifier);
                context.CurrentTargetVariable = symbol.LocalBuilder;
                identifierPattern.InnerPattern.AcceptWalker(this);
            }

            public void VisitIfStatement(IfStatement ifStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitIgnoringRestPattern(IgnoringRestPattern restPattern)
            {
            }

            public void VisitImportDeclaration(ImportDeclaration importDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitIndexerExpression(IndexerExpression indexExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitKeyValuePattern(KeyValuePattern keyValuePattern)
            {
                keyValuePattern.Value.AcceptWalker(this);
            }

            public void VisitKeyValueType(KeyValueType keyValueType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitLiteralExpression(LiteralExpression literal)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitMatchClause(MatchPatternClause matchClause)
            {
                matchClause.Patterns.AcceptWalker(this);
            }

            public void VisitMatchStatement(MatchStatement matchStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitMemberReference(MemberReferenceExpression memRef)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitMemberType(MemberType memberType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitNewLine(NewLineNode newlineNode)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitNullNode(AstNode nullNode)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitNullReferenceExpression(NullReferenceExpression nullRef)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitObjectCreationExpression(ObjectCreationExpression creation)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitPatternWithType(PatternWithType pattern)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitParameterDeclaration(ParameterDeclaration parameterDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitParameterType(ParameterType paramType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitPathExpression(PathExpression pathExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitPatternPlaceholder(AstNode placeholder, Pattern child)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitPlaceholderType(PlaceholderType placeholderType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitPrimitiveType(PrimitiveType primitiveType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitReferenceType(ReferenceType referenceType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitReturnStatement(ReturnStatement returnStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitSequenceExpression(SequenceExpression seqExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitSequenceInitializer(SequenceInitializer seqInitializer)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitSimpleType(SimpleType simpleType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitSuperReferenceExpression(SuperReferenceExpression superRef)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitTextNode(TextNode textNode)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitThrowStatement(ThrowStatement throwStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitTryStatement(TryStatement tryStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitTuplePattern(TuplePattern tuplePattern)
            {
                var tuple_type = context.TemporaryVariable.LocalType;
                generator.EmitLoadLocal(context.TemporaryVariable, false);
                generator.il_generator.Emit(OpCodes.Isinst, tuple_type);
            }

            public void VisitTypeConstraint(TypeConstraint constraint)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitTypeDeclaration(TypeDeclaration typeDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitTypePathPattern(TypePathPattern pathPattern)
            {
            }

            public void VisitUnaryExpression(UnaryExpression unaryExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitVariableInitializer(VariableInitializer initializer)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitWhileStatement(WhileStatement whileStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitWhitespace(WhitespaceNode whitespaceNode)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitWildcardPattern(WildcardPattern wildcardPattern)
            {
            }

            public void VisitYieldStatement(YieldStatement yieldStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }
        }
    }
}
