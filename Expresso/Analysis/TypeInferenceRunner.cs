using System;
using System.Linq;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;
using Expresso.TypeSystem;
using System.Collections.Generic;


namespace Expresso.Ast.Analysis
{
    /// <summary>
    /// The type inference runner is responsible for inferring types when asked.
    /// It does the job by inferring and replacing old nodes with the calculated type nodes
    /// in the symbol table. 
    /// </summary>
    /// <remarks>
    /// Currently, I must say it's just a temporal implementation since it messes around the AST itself
    /// in order to keep track of type relations.
    /// </remarks>
    public class TypeInferenceRunner : IAstWalker<AstType>
    {
        Parser parser;
        SymbolTable symbols;
        TypeInferenceContext context;

        public TypeInferenceRunner(Parser parser, SymbolTable table)
        {
            this.parser = parser;
            symbols = table;
            context = new TypeInferenceContext();
        }

        #region IAstWalker implementation

        public AstType VisitAst(ExpressoAst ast)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitBlock(BlockStatement block)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitBreakStatement(BreakStatement breakStmt)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitContinueStatement(ContinueStatement continueStmt)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitEmptyStatement(EmptyStatement emptyStmt)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitExpressionStatement(ExpressionStatement exprStmt)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitForStatement(ForStatement forStmt)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitIfStatement(IfStatement ifStmt)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitReturnStatement(ReturnStatement returnStmt)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitMatchStatement(MatchStatement matchStmt)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitWhileStatement(WhileStatement whileStmt)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitYieldStatement(YieldStatement yieldStmt)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitAssignment(AssignmentExpression assignment)
        {
            // In an assignment, we want to know the type of the left-hand-side
            // So let's take a look at the right-hand-side
            context.Stack.Push(assignment.Left);
            var right_type = assignment.Right.AcceptWalker(this);
            return right_type;
        }

        public AstType VisitBinaryExpression(BinaryExpression binaryExpr)
        {
            return FigureOutCommonType(binaryExpr.Left, binaryExpr.Right);
        }

        public AstType VisitCallExpression(CallExpression callExpr)
        {
            return callExpr.Target.AcceptWalker(this);
        }

        public AstType VisitCastExpression(CastExpression castExpr)
        {
            return castExpr.ToExpression;
        }

        public AstType VisitComprehensionExpression(ComprehensionExpression comp)
        {
            return comp.ObjectType;
        }

        public AstType VisitComprehensionForClause(ComprehensionForClause compFor)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitComprehensionIfClause(ComprehensionIfClause compIf)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitConditionalExpression(ConditionalExpression condExpr)
        {
            return FigureOutCommonType(condExpr.TrueExpression, condExpr.FalseExpression);
        }

        public AstType VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitLiteralExpression(LiteralExpression literal)
        {
            return literal.Type;
        }

        public AstType VisitIdentifier(Identifier ident)
        {
            return ident.Type;
        }

        public AstType VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq)
        {
            return new PrimitiveType("intseq", TextLocation.Empty);
        }

        public AstType VisitIndexerExpression(IndexerExpression indexExpr)
        {
            var target_type = indexExpr.Target.AcceptWalker(this);
            var simple_type = target_type as SimpleType;
            if(simple_type != null){
                if(simple_type.Identifier != "array")
                    throw new Exception("Can not apply the indexer expression on type `{0}`");

                return simple_type.TypeArguments.FirstOrNullObject();
            }

            return target_type;
        }

        public AstType VisitMemberReference(MemberReference memRef)
        {
            return null;
        }

        public AstType VisitNewExpression(NewExpression newExpr)
        {
            return newExpr.CreationExpression.AcceptWalker(this);
        }

        public AstType VisitPathExpression(PathExpression pathExpr)
        {
            if(pathExpr.Items.Count == 1){
                return pathExpr.AsIdentifier.Type;
            }else{
                var table = symbols;
                AstType result = null;
                foreach(var item in pathExpr.Items){
                    if(table.HasTypeSymbol(item.Name)){
                        var tmp_type = table.GetTypeSymbol(item.Name);
                        result = tmp_type.Type;
                        table = table.Children[0];
                    }else if(table.HasSymbol(item.Name)){
                        var tmp = table.GetTypeSymbol(item.Name);
                        result = tmp.Type;
                        table = table.Children[0];
                    }else{
                        throw new ParserException(
                            "Type or symbol name '{0}' is not declared",
                            item.Name
                        );
                    }
                }

                return result;
            }
        }

        public AstType VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
        {
            return parensExpr.Expression.AcceptWalker(this);
        }

        public AstType VisitObjectCreationExpression(ObjectCreationExpression creation)
        {
            return creation.TypePath.AcceptWalker(this);
        }

        public AstType VisitSequenceInitializer(SequenceInitializer seqInitializer)
        {
            return seqInitializer.ObjectType;
        }

        public AstType VisitMatchClause(MatchPatternClause matchClause)
        {
            return AstType.Null;
        }

        public AstType VisitSequence(SequenceExpression seqExpr)
        {
            // The type of the element of a sequence can be seen as the most common type
            // of the whole sequence.
            var self = this;
            var types = seqExpr.Items.Select(item => (AstType)item.AcceptWalker(self).Clone());
            return new SimpleType("tuple", types, seqExpr.StartLocation, seqExpr.EndLocation);
        }

        public AstType VisitUnaryExpression(UnaryExpression unaryExpr)
        {
            var tmp = unaryExpr.Operand.AcceptWalker(this);
            switch(unaryExpr.Operator){
            case OperatorType.Dereference:
                return null;

            case OperatorType.Reference:
                return null;

            case OperatorType.Plus:
            case OperatorType.Minus:
                {
                    var primitive_type = tmp as PrimitiveType;
                    if(primitive_type != null){
                        if(primitive_type.KnownTypeCode == KnownTypeCode.Int
                            || primitive_type.KnownTypeCode == KnownTypeCode.UInt
                            || primitive_type.KnownTypeCode == KnownTypeCode.Float
                            || primitive_type.KnownTypeCode == KnownTypeCode.Double
                            || primitive_type.KnownTypeCode == KnownTypeCode.Byte
                            || primitive_type.KnownTypeCode == KnownTypeCode.BigInteger)
                            return primitive_type;
                    }

                    throw new ParserException("Can not apply operators '+' or '-' on that type of values.");
                }

            case OperatorType.Not:
                return new PrimitiveType("bool", TextLocation.Empty);

            default:
                throw new ParserException("Unknown unary operator!");
            };
        }

        public AstType VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
        {
            var decl =
                from a in selfRef.Ancestors
                    where a.NodeType == NodeType.TypeDeclaration || a.NodeType == NodeType.Member
                let entity = a as EntityDeclaration
                select entity;

            return decl.First().ReturnType;
        }

        public AstType VisitSuperReferenceExpression(SuperReferenceExpression superRef)
        {
            var decl =
                from a in superRef.Ancestors
                    where a.NodeType == NodeType.TypeReference || a.NodeType == NodeType.Member
                let entity = a as EntityDeclaration
                select entity;

            return decl.First().ReturnType;
        }

        public AstType VisitCommentNode(CommentNode comment)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitTextNode(TextNode textNode)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitSimpleType(SimpleType simpleType)
        {
            return simpleType;
        }

        public AstType VisitPrimitiveType(PrimitiveType primitiveType)
        {
            return primitiveType;
        }

        public AstType VisitReferenceType(ReferenceType referenceType)
        {
            return referenceType.BaseType;
        }

        public AstType VisitMemberType(MemberType memberType)
        {
            return memberType.MemberNameToken.Type;
        }

        public AstType VisitPlaceholderType(PlaceholderType placeholderType)
        {
            return null;
        }

        public AstType VisitAliasDeclaration(AliasDeclaration aliasDecl)
        {
            return aliasDecl.Path.AcceptWalker(this);
        }

        public AstType VisitImportDeclaration(ImportDeclaration importDecl)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitFunctionDeclaration(FunctionDeclaration funcDecl)
        {
            return funcDecl.ReturnType;
        }

        public AstType VisitTypeDeclaration(TypeDeclaration typeDecl)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitFieldDeclaration(FieldDeclaration fieldDecl)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitParameterDeclaration(ParameterDeclaration parameterDecl)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitVariableInitializer(VariableInitializer initializer)
        {
            throw new NotImplementedException();
        }

        public AstType VisitWildcardPattern(WildcardPattern wildcardPattern)
        {
            return SimpleType.Null;
        }

        public AstType VisitIdentifierPattern(IdentifierPattern identifierPattern)
        {
            if(identifierPattern.InnerPattern.IsNull)
                return identifierPattern.Identifier.AcceptWalker(this);
            else
                return identifierPattern.InnerPattern.AcceptWalker(this);
        }

        public AstType VisitValueBindingPattern(ValueBindingPattern valueBindingPattern)
        {
            return valueBindingPattern.Pattern.AcceptWalker(this);
        }

        public AstType VisitTuplePattern(TuplePattern tuplePattern)
        {
            var elem_types = new List<AstType>();
            foreach(var elem in tuplePattern.Patterns)
                elem_types.Add(elem.AcceptWalker(this));

            return new SimpleType("tuple", elem_types, tuplePattern.StartLocation, tuplePattern.EndLocation);
        }

        public AstType VisitCollectionPattern(CollectionPattern collectionPattern)
        {
            throw new NotImplementedException();
        }

        public AstType VisitDestructuringPattern(DestructuringPattern destructuringPattern)
        {
            throw new NotImplementedException();
        }

        public AstType VisitExpressionPattern(ExpressionPattern exprPattern)
        {
            return exprPattern.Expression.AcceptWalker(this);
        }

        public AstType VisitNullNode(AstNode nullNode)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitNewLine(NewLineNode newlineNode)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitWhitespace(WhitespaceNode whitespaceNode)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitExpressoTokenNode(ExpressoTokenNode tokenNode)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public AstType VisitPatternPlaceholder(AstNode placeholder, ICSharpCode.NRefactory.PatternMatching.Pattern child)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        #endregion

        /// <summary>
        /// Given 2 expressions, it tries to figure out the most common type.
        /// </summary>
        /// <returns>The common type between `lhs` and `rhs`.</returns>
        AstType FigureOutCommonType(Expression lhs, Expression rhs)
        {
            var lhs_type = lhs.AcceptWalker(this);
            var rhs_type = rhs.AcceptWalker(this);

            var lhs_primitive = lhs_type as PrimitiveType;
            var rhs_primitive = rhs_type as PrimitiveType;
            if(lhs_primitive != null && rhs_primitive != null){
                // If both are primitives, check if both are numerical types or not
                if(IsNumericalType(lhs_primitive) && IsNumericalType(rhs_primitive)){
                    var common_typecode = lhs_primitive.KnownTypeCode | rhs_primitive.KnownTypeCode;
                    return new PrimitiveType(common_typecode.ToString().ToLower(), TextLocation.Empty);
                }else{
                    // If not, then we must say there is no common types between these 2 expressions
                    return AstType.Null;
                }
            }

            var lhs_simple = lhs_type as SimpleType;
            var rhs_simple = rhs_type as SimpleType;
            if(lhs_simple != null && rhs_simple != null){
                return null;
            }

            parser.ReportWarning(
                "Can not guess the common type between `{0}` and `{1}`.",
                lhs,
                lhs, rhs
            );

            return null;
        }

        static bool IsNumericalType(PrimitiveType type)
        {
            return (int)Expresso.TypeSystem.KnownTypeCode.Int <= (int)type.KnownTypeCode
                && (int)type.KnownTypeCode <= (int)Expresso.TypeSystem.KnownTypeCode.BigInteger;
        }
    }
}

