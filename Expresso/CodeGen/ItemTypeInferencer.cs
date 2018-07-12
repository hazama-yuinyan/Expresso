using System;
using System.Linq;
using Expresso.Ast;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.CodeGen
{
    public partial class CodeGenerator : IAstWalker<CSharpEmitterContext, Type>
    {
        /// <summary>
        /// This class is responsible for infering the type of a pattern.
        /// </summary>
        public class ItemTypeInferencer : IAstWalker<AstType>
	    {
            CodeGenerator generator;

            public ItemTypeInferencer(CodeGenerator generator)
	        {
                this.generator = generator;
	        }

            public AstType VisitAliasDeclaration(AliasDeclaration aliasDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitAssignment(AssignmentExpression assignment)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitAst(ExpressoAst ast)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitAttributeSection(AttributeSection section)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitBinaryExpression(BinaryExpression binaryExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitBlock(BlockStatement block)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitBreakStatement(BreakStatement breakStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitCallExpression(CallExpression call)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitCastExpression(CastExpression castExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitCatchClause(CatchClause catchClause)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitClosureLiteralExpression(ClosureLiteralExpression closure)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitCollectionPattern(CollectionPattern collectionPattern)
            {
                return collectionPattern.CollectionType;
            }

            public AstType VisitCommentNode(CommentNode comment)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitComprehensionExpression(ComprehensionExpression comp)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitComprehensionForClause(ComprehensionForClause compFor)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitComprehensionIfClause(ComprehensionIfClause compIf)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitConditionalExpression(ConditionalExpression condExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitContinueStatement(ContinueStatement continueStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitDestructuringPattern(DestructuringPattern destructuringPattern)
            {
                return destructuringPattern.TypePath;
            }

            public AstType VisitDoWhileStatement(DoWhileStatement doWhileStmt)
            {
                throw new InvalidOperationException("can not work on that node");
            }

            public AstType VisitEmptyStatement(EmptyStatement emptyStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitExpressionPattern(ExpressionPattern exprPattern)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitExpressionStatement(ExpressionStatement exprStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitExpressoTokenNode(ExpressoTokenNode tokenNode)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitFieldDeclaration(FieldDeclaration fieldDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitFinallyClause(FinallyClause finallyClause)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitForStatement(ForStatement forStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitFunctionDeclaration(FunctionDeclaration funcDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitFunctionType(FunctionType funcType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitIdentifier(Identifier ident)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitIdentifierPattern(IdentifierPattern identifierPattern)
            {
                return identifierPattern.Identifier.Type;
                /*var type = identifierPattern.Parent.AcceptWalker(this);
                var simple_type = type as SimpleType;
                if(IsContainerType(type)){
                    var elem_type = simple_type.TypeArguments.FirstOrNullObject();
                    return elem_type;
                }else if(IsTupleType(type)){
                    int index = 0;
                    identifierPattern.Parent.Children.Any(pattern => {
                        if(pattern.IsMatch(identifierPattern)){
                            return false;
                        }else{
                            ++index;
                            return true;
                        }
                    });
                    var elem_type = simple_type.TypeArguments.ElementAt(index);
                    return elem_type;
                }else if(simple_type != null){
                    // This is the case where type represents a user-defined type
                    var type_table = emitter.symbol_table.GetTypeTable(simple_type.Identifier);
                    if(type_table == null){
                        throw new EmitterException("Type {0} isn't defined.", simple_type.Identifier);
                    }

                    var attr_type = type_table.GetSymbol(identifierPattern.Identifier.Name);
                    return attr_type.Type;
                }else if(IsIntSeqType(type)){
                    return AstType.MakePrimitiveType("int");
                }else{
                    return type;
                }*/
            }

            public AstType VisitIfStatement(IfStatement ifStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitIgnoringRestPattern(IgnoringRestPattern restPattern)
            {
                return null;
            }

            public AstType VisitImportDeclaration(ImportDeclaration importDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitIndexerExpression(IndexerExpression indexExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitKeyValuePattern(KeyValuePattern keyValuePattern)
            {
                return keyValuePattern.KeyIdentifier.Type;
            }

            public AstType VisitKeyValueType(KeyValueType keyValueType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitLiteralExpression(LiteralExpression literal)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitMatchClause(MatchPatternClause matchClause)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitMatchStatement(MatchStatement matchStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitMemberReference(MemberReferenceExpression memRef)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitMemberType(MemberType memberType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitNewLine(NewLineNode newlineNode)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitNullNode(AstNode nullNode)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitNullReferenceExpression(NullReferenceExpression nullRef)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitObjectCreationExpression(ObjectCreationExpression creation)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitPatternWithType(PatternWithType pattern)
            {
                return pattern.Type;
            }

            public AstType VisitParameterDeclaration(ParameterDeclaration parameterDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitParameterType(ParameterType paramType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitPathExpression(PathExpression pathExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitPatternPlaceholder(AstNode placeholder, Pattern child)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitPlaceholderType(PlaceholderType placeholderType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitPrimitiveType(PrimitiveType primitiveType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitReferenceType(ReferenceType referenceType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitReturnStatement(ReturnStatement returnStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitSequenceExpression(SequenceExpression seqExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitSequenceInitializer(SequenceInitializer seqInitializer)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitSimpleType(SimpleType simpleType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitSuperReferenceExpression(SuperReferenceExpression superRef)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitTextNode(TextNode textNode)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitThrowStatement(ThrowStatement throwStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitTryStatement(TryStatement tryStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitTuplePattern(TuplePattern tuplePattern)
            {
                var item_types = tuplePattern.Patterns.Select(pat => pat.AcceptWalker(this));
                return AstType.MakeSimpleType("tuple", item_types);
            }

            public AstType VisitTypeConstraint(TypeConstraint constraint)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitTypeDeclaration(TypeDeclaration typeDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitTypePathPattern(TypePathPattern pathPattern)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitUnaryExpression(UnaryExpression unaryExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStatment)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitVariableInitializer(VariableInitializer initializer)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitWhileStatement(WhileStatement whileStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitWhitespace(WhitespaceNode whitespaceNode)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitWildcardPattern(WildcardPattern wildcardPattern)
            {
                return null;
            }

            public AstType VisitYieldStatement(YieldStatement yieldStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }
        }
    }
}
