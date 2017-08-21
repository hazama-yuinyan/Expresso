using System;
using System.Linq;
using Expresso.Ast;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.CodeGen
{
    using CSharpExpr = System.Linq.Expressions.Expression;

    public partial class CSharpEmitter : IAstWalker<CSharpEmitterContext, CSharpExpr>
    {
        /// <summary>
        /// This class is responsible for infering the type of a pattern.
        /// </summary>
        public class ItemTypeInferencer : IAstWalker<AstType>
	    {
            CSharpEmitter emitter;

            public ItemTypeInferencer(CSharpEmitter emitter)
	        {
                this.emitter = emitter;
	        }

            public AstType VisitAliasDeclaration(AliasDeclaration aliasDecl)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitAssignment(AssignmentExpression assignment)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitAst(ExpressoAst ast)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitBinaryExpression(BinaryExpression binaryExpr)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitBlock(BlockStatement block)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitBreakStatement(BreakStatement breakStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitCallExpression(CallExpression call)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitCastExpression(CastExpression castExpr)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitCatchClause(CatchClause catchClause)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitClosureLiteralExpression(ClosureLiteralExpression closure)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitCollectionPattern(CollectionPattern collectionPattern)
            {
                return collectionPattern.CollectionType;
            }

            public AstType VisitCommentNode(CommentNode comment)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitComprehensionExpression(ComprehensionExpression comp)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitComprehensionForClause(ComprehensionForClause compFor)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitComprehensionIfClause(ComprehensionIfClause compIf)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitConditionalExpression(ConditionalExpression condExpr)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitContinueStatement(ContinueStatement continueStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitDestructuringPattern(DestructuringPattern destructuringPattern)
            {
                return destructuringPattern.TypePath;
            }

            public AstType VisitEmptyStatement(EmptyStatement emptyStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitExpressionPattern(ExpressionPattern exprPattern)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitExpressionStatement(ExpressionStatement exprStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitExpressoTokenNode(ExpressoTokenNode tokenNode)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitFieldDeclaration(FieldDeclaration fieldDecl)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitForStatement(ForStatement forStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitFunctionDeclaration(FunctionDeclaration funcDecl)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitFunctionType(FunctionType funcType)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitIdentifier(Identifier ident)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitIdentifierPattern(IdentifierPattern identifierPattern)
            {
                var type = identifierPattern.Parent.AcceptWalker(this);
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
                }
            }

            public AstType VisitIfStatement(IfStatement ifStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitIgnoringRestPattern(IgnoringRestPattern restPattern)
            {
                return null;
            }

            public AstType VisitImportDeclaration(ImportDeclaration importDecl)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitIndexerExpression(IndexerExpression indexExpr)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitKeyValuePattern(KeyValuePattern keyValuePattern)
            {
                return keyValuePattern.KeyIdentifier.Type;
            }

            public AstType VisitLiteralExpression(LiteralExpression literal)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitMatchClause(MatchPatternClause matchClause)
            {
                return matchClause.Parent.AcceptWalker(this);
            }

            public AstType VisitMatchStatement(MatchStatement matchStmt)
            {
                return matchStmt.Target.AcceptWalker(this);
            }

            public AstType VisitMemberReference(MemberReferenceExpression memRef)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitMemberType(MemberType memberType)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitNewExpression(NewExpression newExpr)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitNewLine(NewLineNode newlineNode)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitNullNode(AstNode nullNode)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitObjectCreationExpression(ObjectCreationExpression creation)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitParameterDeclaration(ParameterDeclaration parameterDecl)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitParameterType(ParameterType paramType)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitPathExpression(PathExpression pathExpr)
            {
                var type = pathExpr.Items.Select(item => item.Type).Last();
                return type;
            }

            public AstType VisitPatternPlaceholder(AstNode placeholder, Pattern child)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitPlaceholderType(PlaceholderType placeholderType)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitPrimitiveType(PrimitiveType primitiveType)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitReferenceType(ReferenceType referenceType)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitReturnStatement(ReturnStatement returnStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitSequenceExpression(SequenceExpression seqExpr)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitSequenceInitializer(SequenceInitializer seqInitializer)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitSimpleType(SimpleType simpleType)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitSuperReferenceExpression(SuperReferenceExpression superRef)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitTextNode(TextNode textNode)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitThrowStatement(ThrowStatement throwStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitTryStatement(TryStatement tryStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitTuplePattern(TuplePattern tuplePattern)
            {
                return tuplePattern.Parent.AcceptWalker(this);
            }

            public AstType VisitTypeDeclaration(TypeDeclaration typeDecl)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitUnaryExpression(UnaryExpression unaryExpr)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStatment)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitValueBindingPattern(ValueBindingPattern valueBindingPattern)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitVariableInitializer(VariableInitializer initializer)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitWhileStatement(WhileStatement whileStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitWhitespace(WhitespaceNode whitespaceNode)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public AstType VisitWildcardPattern(WildcardPattern wildcardPattern)
            {
                return null;
            }

            public AstType VisitYieldStatement(YieldStatement yieldStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            static bool IsContainerType(AstType type)
            {
                var simple = type as SimpleType;
                if(simple != null && (simple.Identifier == "array" || simple.Identifier == "vector"))
                    return true;
                else
                    return false;
            }

            static bool IsTupleType(AstType type)
            {
                var simple = type as SimpleType;
                if(simple != null && simple.Identifier == "tuple")
                    return true;
                else
                    return false;
            }

            static bool IsIntSeqType(AstType type)
            {
                var primitive = type as PrimitiveType;
                if(primitive != null && primitive.KnownTypeCode == TypeSystem.KnownTypeCode.IntSeq)
                    return true;
                else
                    return false;
            }
        }
    }
}
