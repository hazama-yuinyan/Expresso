using System;
using Expresso.Ast;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;


namespace Expresso.CodeGen
{
    using CSharpExpr = System.Linq.Expressions.Expression;

    public partial class CSharpEmitter : IAstWalker<CSharpEmitterContext, CSharpExpr>
    {
        /// <summary>
        /// An InterfaceTypeDefiner is responsible for inspecting types and defining the outlines of types.
        /// </summary>
        public class InterfaceTypeDefiner : IAstWalker
        {
            CSharpEmitter emitter;
            CSharpEmitterContext context;

            public InterfaceTypeDefiner(CSharpEmitter emitter, CSharpEmitterContext context)
            {
                this.emitter = emitter;
                this.context = context;
            }

            #region IAstWalker implementation

            public void VisitAst(ExpressoAst ast)
            {
                throw new InvalidOperationException();
            }

            public void VisitBlock(BlockStatement block)
            {
                throw new InvalidOperationException();
            }

            public void VisitBreakStatement(BreakStatement breakStmt)
            {
                throw new InvalidOperationException();
            }

            public void VisitContinueStatement(ContinueStatement continueStmt)
            {
                throw new InvalidOperationException();
            }

            public void VisitDoWhileStatement(DoWhileStatement doWhileStmt)
            {
                throw new InvalidOperationException();
            }

            public void VisitEmptyStatement(EmptyStatement emptyStmt)
            {
                throw new InvalidOperationException();
            }

            public void VisitExpressionStatement(ExpressionStatement exprStmt)
            {
                throw new InvalidOperationException();
            }

            public void VisitForStatement(ForStatement forStmt)
            {
                throw new InvalidOperationException();
            }

            public void VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStatement)
            {
                throw new InvalidOperationException();
            }

            public void VisitIfStatement(IfStatement ifStmt)
            {
                throw new InvalidOperationException();
            }

            public void VisitReturnStatement(ReturnStatement returnStmt)
            {
                throw new InvalidOperationException();
            }

            public void VisitMatchStatement(MatchStatement matchStmt)
            {
                throw new InvalidOperationException();
            }

            public void VisitThrowStatement(ThrowStatement throwStmt)
            {
                throw new InvalidOperationException();
            }

            public void VisitTryStatement(TryStatement tryStmt)
            {
                throw new InvalidOperationException();
            }

            public void VisitWhileStatement(WhileStatement whileStmt)
            {
                throw new InvalidOperationException();
            }

            public void VisitYieldStatement(YieldStatement yieldStmt)
            {
                throw new InvalidOperationException();
            }

            public void VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
            {
                throw new InvalidOperationException();
            }

            public void VisitAssignment(AssignmentExpression assignment)
            {
                throw new InvalidOperationException();
            }

            public void VisitBinaryExpression(BinaryExpression binaryExpr)
            {
                throw new InvalidOperationException();
            }

            public void VisitCallExpression(CallExpression call)
            {
                throw new InvalidOperationException();
            }

            public void VisitCastExpression(CastExpression castExpr)
            {
                throw new InvalidOperationException();
            }

            public void VisitCatchClause(CatchClause catchClause)
            {
                throw new InvalidOperationException();
            }

            public void VisitClosureLiteralExpression(ClosureLiteralExpression closure)
            {
                throw new InvalidOperationException();
            }

            public void VisitComprehensionExpression(ComprehensionExpression comp)
            {
                throw new InvalidOperationException();
            }

            public void VisitComprehensionForClause(ComprehensionForClause compFor)
            {
                throw new InvalidOperationException();
            }

            public void VisitComprehensionIfClause(ComprehensionIfClause compIf)
            {
                throw new InvalidOperationException();
            }

            public void VisitConditionalExpression(ConditionalExpression condExpr)
            {
                throw new InvalidOperationException();
            }

            public void VisitFinallyClause(FinallyClause finallyClause)
            {
                throw new InvalidOperationException();
            }

            public void VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
            {
                throw new InvalidOperationException();
            }

            public void VisitLiteralExpression(LiteralExpression literal)
            {
                throw new InvalidOperationException();
            }

            public void VisitIdentifier(Identifier ident)
            {
                throw new InvalidOperationException();
            }

            public void VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq)
            {
                throw new InvalidOperationException();
            }

            public void VisitIndexerExpression(IndexerExpression indexExpr)
            {
                throw new InvalidOperationException();
            }

            public void VisitMemberReference(MemberReferenceExpression memRef)
            {
                throw new InvalidOperationException();
            }

            public void VisitPathExpression(PathExpression pathExpr)
            {
                throw new InvalidOperationException();
            }

            public void VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
            {
                throw new InvalidOperationException();
            }

            public void VisitObjectCreationExpression(ObjectCreationExpression creation)
            {
                throw new InvalidOperationException();
            }

            public void VisitSequenceInitializer(SequenceInitializer seqInitializer)
            {
                throw new InvalidOperationException();
            }

            public void VisitMatchClause(MatchPatternClause matchClause)
            {
                throw new InvalidOperationException();
            }

            public void VisitSequenceExpression(SequenceExpression seqExpr)
            {
                throw new InvalidOperationException();
            }

            public void VisitUnaryExpression(UnaryExpression unaryExpr)
            {
                throw new InvalidOperationException();
            }

            public void VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
            {
                throw new InvalidOperationException();
            }

            public void VisitSuperReferenceExpression(SuperReferenceExpression superRef)
            {
                throw new InvalidOperationException();
            }

            public void VisitNullReferenceExpression(NullReferenceExpression nullRef)
            {
                throw new InvalidOperationException();
            }

            public void VisitCommentNode(CommentNode comment)
            {
                throw new InvalidOperationException();
            }

            public void VisitTextNode(TextNode textNode)
            {
                throw new InvalidOperationException();
            }

            public void VisitSimpleType(SimpleType simpleType)
            {
                throw new InvalidOperationException();
            }

            public void VisitPrimitiveType(PrimitiveType primitiveType)
            {
                throw new InvalidOperationException();
            }

            public void VisitReferenceType(ReferenceType referenceType)
            {
                throw new InvalidOperationException();
            }

            public void VisitMemberType(MemberType memberType)
            {
                throw new InvalidOperationException();
            }

            public void VisitFunctionType(FunctionType funcType)
            {
                throw new InvalidOperationException();
            }

            public void VisitParameterType(ParameterType paramType)
            {
                throw new InvalidOperationException();
            }

            public void VisitPlaceholderType(PlaceholderType placeholderType)
            {
                throw new InvalidOperationException();
            }

            public void VisitImportDeclaration(ImportDeclaration importDecl)
            {
                throw new InvalidOperationException();
            }

            public void VisitFunctionDeclaration(FunctionDeclaration funcDecl)
            {
                context.Additionals = new List<object>();
                int tmp_counter = emitter.scope_counter;
                emitter.DescendScope();
                emitter.scope_counter = 0;

                var context_ast = context.ContextAst;
                context.ContextAst = funcDecl;

                var param_types = funcDecl.Parameters.Select(param => param.AcceptWalker(emitter, context).Type);

                var return_type = CSharpCompilerHelper.GetNativeType(funcDecl.ReturnType);

                var attr = (context.LazyTypeBuilder != null) ? MethodAttributes.Private : MethodAttributes.Static | MethodAttributes.Private;
                if(funcDecl.Modifiers.HasFlag(Modifiers.Public)){
                    attr |= MethodAttributes.Public;
                    attr ^= MethodAttributes.Private;
                }

                if(funcDecl.Parent is TypeDeclaration type_decl && type_decl.TypeKind == ClassType.Interface){
                    attr |= MethodAttributes.Abstract | MethodAttributes.Virtual;
                }else if(funcDecl.Parent is TypeDeclaration type_decl2){
                    foreach(var base_type in type_decl2.BaseTypes){
                        var native_type = Symbols[base_type.IdentifierNode.IdentifierId].Type;
                        // interfaces methods must be virtual
                        if(native_type.GetMethod(CSharpCompilerHelper.ConvertToPascalCase(funcDecl.Name)) != null)
                            attr |= MethodAttributes.Virtual;
                    }
                }

                if(funcDecl.Parent is TypeDeclaration type_decl3 && type_decl3.TypeKind == ClassType.Interface)
                    context.InterfaceTypeBuilder.DefineMethod(CSharpCompilerHelper.ConvertToPascalCase(funcDecl.Name), attr, return_type, param_types.ToArray());
                else
                    context.LazyTypeBuilder.DefineMethod(CSharpCompilerHelper.ConvertToPascalCase(funcDecl.Name), attr, return_type, param_types.ToArray());

                emitter.AscendScope();
                emitter.scope_counter = tmp_counter + 1;
            }

            public void VisitTypeDeclaration(TypeDeclaration typeDecl)
            {
                int tmp_counter = emitter.scope_counter;
                emitter.DescendScope();
                emitter.scope_counter = 0;

                var parent_type = context.LazyTypeBuilder;
                var attr = typeDecl.Modifiers.HasFlag(Modifiers.Export) ? TypeAttributes.Public : TypeAttributes.NotPublic;
                attr |= (typeDecl.TypeKind == ClassType.Interface) ? TypeAttributes.Interface | TypeAttributes.Abstract : TypeAttributes.Class | TypeAttributes.BeforeFieldInit;
                var name = typeDecl.Name;

                foreach(var base_type in typeDecl.BaseTypes){
                    if(base_type is SimpleType simple){
                        var native_type = CSharpCompilerHelper.GetNativeType(!simple.IdentifierNode.Type.IsNull ? simple.IdentifierNode.Type : simple);
                        if(!Symbols.ContainsKey(base_type.IdentifierNode.IdentifierId))
                            AddSymbol(base_type.IdentifierNode, new ExpressoSymbol{Type = native_type});
                    }
                }

                var base_types = 
                    from bt in typeDecl.BaseTypes
                    select Symbols[bt.IdentifierNode.IdentifierId].Type;

                // derive from object if we don't already derive from that
                var first_type = base_types.FirstOrDefault();
                if(first_type != null && !first_type.IsClass)
                    base_types = new []{typeof(object)}.Concat(base_types);
                
                if(typeDecl.TypeKind == ClassType.Interface)
                    context.InterfaceTypeBuilder = context.ModuleBuilder.DefineType(name, TypeAttributes.Interface | TypeAttributes.Abstract);
                else
                    context.LazyTypeBuilder = new LazyTypeBuilder(context.ModuleBuilder, name, attr, base_types, false);

                foreach(var base_type in base_types)
                    context.LazyTypeBuilder.InterfaceTypeBuilder.AddInterfaceImplementation(base_type);

                try{
                    foreach(var member in typeDecl.Members)
                        member.AcceptWalker(this);

                    var type = (typeDecl.TypeKind == ClassType.Interface) ? context.InterfaceTypeBuilder.CreateType() : context.LazyTypeBuilder.CreateInterfaceType();
                    var expresso_symbol = (typeDecl.TypeKind == ClassType.Interface) ? new ExpressoSymbol{Type = type} : new ExpressoSymbol{Type = type, TypeBuilder = context.LazyTypeBuilder};
                    AddSymbol(typeDecl.NameToken, expresso_symbol);
                }
                finally{
                    context.LazyTypeBuilder = parent_type;
                    context.InterfaceTypeBuilder = null;
                }

                emitter.AscendScope();
                emitter.scope_counter = tmp_counter + 1;
            }

            public void VisitAliasDeclaration(AliasDeclaration aliasDecl)
            {
                throw new InvalidOperationException();
            }

            public void VisitFieldDeclaration(FieldDeclaration fieldDecl)
            {
                FieldAttributes attr = FieldAttributes.Private;
                if(fieldDecl.Modifiers.HasFlag(Modifiers.Static))
                    attr |= FieldAttributes.Static;

                // Don't set InitOnly flag or we'll fail to initialize the fields
                //if(fieldDecl.Modifiers.HasFlag(Modifiers.Immutable))
                //    attr |= FieldAttributes.InitOnly;

                if(fieldDecl.Modifiers.HasFlag(Modifiers.Private))
                    attr |= FieldAttributes.Private;
                else if(fieldDecl.Modifiers.HasFlag(Modifiers.Protected))
                    attr |= FieldAttributes.Family;
                else if(fieldDecl.Modifiers.HasFlag(Modifiers.Public))
                    attr |= FieldAttributes.Public;
                else
                    throw new EmitterException("Unknown modifiers!");

                if(!fieldDecl.Modifiers.HasFlag(Modifiers.Private))
                    attr ^= FieldAttributes.Private;

                foreach(var init in fieldDecl.Initializers){
                    var type = CSharpCompilerHelper.GetNativeType(init.NameToken.Type);
                    var field_builder = context.LazyTypeBuilder.DefineField(init.Name, type, !Expression.IsNullNode(init.Initializer), attr);
                    AddSymbol(init.NameToken, new ExpressoSymbol{PropertyOrField = field_builder});
                }
            }

            public void VisitParameterDeclaration(ParameterDeclaration parameterDecl)
            {
                throw new InvalidOperationException();
            }

            public void VisitVariableInitializer(VariableInitializer initializer)
            {
                throw new InvalidOperationException();
            }

            public void VisitWildcardPattern(WildcardPattern wildcardPattern)
            {
                throw new InvalidOperationException();
            }

            public void VisitIdentifierPattern(IdentifierPattern identifierPattern)
            {
                throw new InvalidOperationException();
            }

            public void VisitCollectionPattern(CollectionPattern collectionPattern)
            {
                throw new InvalidOperationException();
            }

            public void VisitDestructuringPattern(DestructuringPattern destructuringPattern)
            {
                throw new InvalidOperationException();
            }

            public void VisitTuplePattern(TuplePattern tuplePattern)
            {
                throw new InvalidOperationException();
            }

            public void VisitExpressionPattern(ExpressionPattern exprPattern)
            {
                throw new InvalidOperationException();
            }

            public void VisitIgnoringRestPattern(IgnoringRestPattern restPattern)
            {
                throw new InvalidOperationException();
            }

            public void VisitKeyValuePattern(KeyValuePattern keyValuePattern)
            {
                throw new InvalidOperationException();
            }

            public void VisitPatternWithType(PatternWithType pattern)
            {
                throw new InvalidOperationException();
            }

            public void VisitNullNode(AstNode nullNode)
            {
                throw new InvalidOperationException();
            }

            public void VisitNewLine(NewLineNode newlineNode)
            {
                throw new InvalidOperationException();
            }

            public void VisitWhitespace(WhitespaceNode whitespaceNode)
            {
                throw new InvalidOperationException();
            }

            public void VisitExpressoTokenNode(ExpressoTokenNode tokenNode)
            {
                throw new InvalidOperationException();
            }

            public void VisitPatternPlaceholder(AstNode placeholder, ICSharpCode.NRefactory.PatternMatching.Pattern child)
            {
                throw new InvalidOperationException();
            }

            #endregion
        }
    }
}

