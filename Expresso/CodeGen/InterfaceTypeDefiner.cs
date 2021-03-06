﻿using System;
using Expresso.Ast;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace Expresso.CodeGen
{
    public partial class CodeGenerator : IAstWalker<CSharpEmitterContext, Type>
    {
        /// <summary>
        /// An InterfaceTypeDefiner is responsible for inspecting types and defining the outlines of types.
        /// </summary>
        public class InterfaceTypeDefiner : IAstWalker
        {
            CodeGenerator generator;
            CSharpEmitterContext context;
            string field_prefix;
            Type self_type;

            public InterfaceTypeDefiner(CodeGenerator generator, CSharpEmitterContext context)
            {
                this.generator = generator;
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

            public void VisitTypeConstraint(TypeConstraint constraint)
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

            public void VisitKeyValueType(KeyValueType keyValueType)
            {
                throw new InvalidOperationException();
            }

            public void VisitPlaceholderType(PlaceholderType placeholderType)
            {
                throw new InvalidOperationException();
            }

            public void VisitAttributeSection(AttributeSection section)
            {
                throw new InvalidOperationException();
            }

            public void VisitImportDeclaration(ImportDeclaration importDecl)
            {
                throw new InvalidOperationException();
            }

            public void VisitFunctionDeclaration(FunctionDeclaration funcDecl)
            {
                int tmp_counter = generator.scope_counter;
                generator.DescendScope();
                generator.scope_counter = 0;

                var context_ast = context.ContextAst;
                context.ContextAst = funcDecl;

                var param_types = funcDecl.Parameters.Select((param, index) => {
                    context.ParameterIndex = index + 1;
                    return generator.VisitParameterDeclaration(param, context);
                });

                var return_type = CSharpCompilerHelpers.GetNativeType(funcDecl.ReturnType);

                var flags = (context.LazyTypeBuilder != null) ? MethodAttributes.Private : MethodAttributes.Static | MethodAttributes.Private;
                if(funcDecl.Modifiers.HasFlag(Modifiers.Public)){
                    flags |= MethodAttributes.Public;
                    flags ^= MethodAttributes.Private;
                }

                if(funcDecl.Parent is TypeDeclaration type_decl && type_decl.TypeKind == ClassType.Interface){
                    flags |= MethodAttributes.Abstract | MethodAttributes.Virtual;
                }else if(funcDecl.Parent is TypeDeclaration type_decl2){
                    foreach(var base_type in type_decl2.BaseTypes){
                        var native_type = Symbols[base_type.IdentifierNode.IdentifierId].Type;
                        // interfaces methods must be virtual
                        if(native_type.GetMethod(funcDecl.Name) != null)
                            flags |= MethodAttributes.Virtual;
                    }
                }

                MethodBuilder method_builder;
                if(funcDecl.Parent is TypeDeclaration type_decl3 && type_decl3.TypeKind == ClassType.Interface)
                    method_builder = context.InterfaceTypeBuilder.DefineMethod(funcDecl.Name, flags, return_type, param_types.ToArray());
                else
                    method_builder = context.LazyTypeBuilder.DefineMethod(funcDecl.Name, flags, return_type, param_types.ToArray(), generator, context, funcDecl);

                context.CustomAttributeSetter = method_builder.SetCustomAttribute;
                context.AttributeTarget = AttributeTargets.Method;
                // We don't call VisitAttributeSection directly so that we can avoid unnecessary method calls
                funcDecl.Attribute.AcceptWalker(generator, context);

                generator.AscendScope();
                generator.scope_counter = tmp_counter + 1;
            }

            public void VisitTypeDeclaration(TypeDeclaration typeDecl)
            {
                int tmp_counter = generator.scope_counter;
                generator.DescendScope();
                generator.scope_counter = 0;

                var parent_type = context.LazyTypeBuilder;
                var flags = typeDecl.Modifiers.HasFlag(Modifiers.Export) ? TypeAttributes.Public : TypeAttributes.NotPublic;
                flags |= (typeDecl.TypeKind == ClassType.Interface) ? TypeAttributes.Interface | TypeAttributes.Abstract : TypeAttributes.Class | TypeAttributes.BeforeFieldInit;
                var name = typeDecl.Name;

                foreach(var base_type in typeDecl.BaseTypes){
                    if(base_type is SimpleType simple){
                        var native_type = CSharpCompilerHelpers.GetNativeType(!simple.IdentifierNode.Type.IsNull ? simple.IdentifierNode.Type : simple);
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

                var is_raw_value_enum = generator.symbol_table.GetSymbol(Utilities.RawValueEnumValueFieldName) != null;
                if(typeDecl.TypeKind == ClassType.Interface){
                    context.InterfaceTypeBuilder = context.ModuleBuilder.DefineType(name, TypeAttributes.Interface | TypeAttributes.Abstract);
                    if(typeDecl.TypeConstraints.Any())
                        generator.generic_types = context.InterfaceTypeBuilder.DefineGenericParameters(typeDecl.TypeConstraints.Select(c => c.TypeParameter.Name).ToArray());
                    
                    context.CustomAttributeSetter = context.InterfaceTypeBuilder.SetCustomAttribute;
                    // We don't call VisitAttributeSection directly so that we can avoid unnecessary method calls
                    typeDecl.Attribute.AcceptWalker(generator, context);
                }else{
                    context.LazyTypeBuilder = new WrappedTypeBuilder(context.ModuleBuilder, name, flags, base_types, false, is_raw_value_enum);
                    //if(typeDecl.TypeConstraints.Any())
                    //    generator.generic_types = context.LazyTypeBuilder.TypeBuilder.DefineGenericParameters(typeDecl.TypeConstraints.Select(c => c.TypeParameter.Name).ToArray());

                    context.CustomAttributeSetter = context.LazyTypeBuilder.TypeBuilder.SetCustomAttribute;
                    context.AttributeTarget = AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum;
                    // We don't call VisitAttributeSection directly so that we can avoid unnecessary method calls
                    typeDecl.Attribute.AcceptWalker(generator, context);

                    if(typeDecl.TypeKind == ClassType.Enum)
                        field_prefix = "<>__";
                    else
                        field_prefix = "";
                }

                foreach(var base_type in base_types){
                    if(base_type.IsInterface)
                        context.LazyTypeBuilder.TypeBuilder.AddInterfaceImplementation(base_type);
                }

                if(typeDecl.TypeKind != ClassType.Enum || !is_raw_value_enum){
                    try{
                        foreach(var member in typeDecl.Members)
                            member.AcceptWalker(this);

                        var type = (typeDecl.TypeKind == ClassType.Interface) ? context.InterfaceTypeBuilder.CreateType() : context.LazyTypeBuilder.CreateInterfaceType();
                        var expresso_symbol = (typeDecl.TypeKind == ClassType.Interface) ? new ExpressoSymbol{Type = type} : new ExpressoSymbol{Type = type, TypeBuilder = context.LazyTypeBuilder};
                        AddSymbol(typeDecl.NameToken, expresso_symbol);

                        if(typeDecl.TypeKind != ClassType.Interface){
                            // Add fields as ExpressoSymbols so that we can refer to fields easily
                            RegisterFields(typeDecl, type);
                        }
                    }
                    finally{
                        context.LazyTypeBuilder = parent_type;
                        context.InterfaceTypeBuilder = null;
                    }
                }else{
                    try{
                        var interface_type_builder = context.LazyTypeBuilder;
                        var type_builder = interface_type_builder.DefineNestedType("<RealEnum>", TypeAttributes.NotPublic | TypeAttributes.Class, new []{typeof(Enum)});
                        self_type = type_builder.TypeAsType;

                        context.LazyTypeBuilder = type_builder;
                        foreach(var member in typeDecl.Members.OfType<FieldDeclaration>())
                            member.AcceptWalker(this);

                        context.LazyTypeBuilder = interface_type_builder;
                        foreach(var method in typeDecl.Members.OfType<FunctionDeclaration>())
                            method.AcceptWalker(this);

                        // Define value__ field on the real enum type
                        // It seems to be needed so that it will be recognized as an enum
                        type_builder.DefineField("value__", typeof(int), false, FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName);

                        var enum_interface_type = type_builder.CreateInterfaceType();
                        type_builder.CreateType();

                        var field_builder = interface_type_builder.DefineField(Utilities.RawValueEnumValueFieldName, enum_interface_type, false);
                        var value_symbol = generator.symbol_table.GetSymbol(Utilities.RawValueEnumValueFieldName);
                        AddSymbol(value_symbol, new ExpressoSymbol{PropertyOrField = field_builder});

                        var interface_interface_type = interface_type_builder.CreateInterfaceType();
                        var expresso_symbol = new ExpressoSymbol{Type = interface_interface_type, TypeBuilder = interface_type_builder};
                        AddSymbol(typeDecl.NameToken, expresso_symbol);

                        // Add fields as ExpressoSymbols so that we can refer to raw value style enum members
                        // This operation can't be moved to CodeGenerator because we have no reference to the enum interface type there
                        RegisterFields(typeDecl, enum_interface_type);
                    }
                    finally{
                        context.LazyTypeBuilder = parent_type;
                        self_type = null;
                    }
                }

                generator.AscendScope();
                generator.scope_counter = tmp_counter + 1;
            }

            public void VisitAliasDeclaration(AliasDeclaration aliasDecl)
            {
                throw new InvalidOperationException();
            }

            public void VisitFieldDeclaration(FieldDeclaration fieldDecl)
            {
                var flags = FieldAttributes.Private;
                if(fieldDecl.Modifiers.HasFlag(Modifiers.Static))
                    flags |= FieldAttributes.Static;

                // Don't set InitOnly flag or we'll fail to initialize the fields
                // because fields are initialized via impl methods.
                // Therefore they won't be closed in the consturctor.
                //if(fieldDecl.Modifiers.HasFlag(Modifiers.Immutable))
                //    attr |= FieldAttributes.InitOnly;

                if(fieldDecl.Modifiers.HasFlag(Modifiers.Private))
                    flags |= FieldAttributes.Private;
                else if(fieldDecl.Modifiers.HasFlag(Modifiers.Protected))
                    flags |= FieldAttributes.Family;
                else if(fieldDecl.Modifiers.HasFlag(Modifiers.Public))
                    flags |= FieldAttributes.Public;
                else
                    throw new EmitterException("Unknown modifiers!");

                if(!fieldDecl.Modifiers.HasFlag(Modifiers.Private))
                    flags ^= FieldAttributes.Private;

                foreach(var init in fieldDecl.Initializers){
                    var type = DetermineType(init.NameToken.Type);
                    var field_builder = context.LazyTypeBuilder.DefineField(field_prefix + init.Name, type, !Expression.IsNullNode(init.Initializer), flags);

                    context.CustomAttributeSetter = field_builder.SetCustomAttribute;
                    context.AttributeTarget = AttributeTargets.Field;
                    // We don't call VisitAttributeSection directly so that we can avoid unnecessary method calls
                    fieldDecl.Attribute.AcceptWalker(generator, context);

                    AddSymbol(init.NameToken, new ExpressoSymbol{FieldBuilder = field_builder});
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

            public void VisitTypePathPattern(TypePathPattern pathPattern)
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

            void RegisterFields(TypeDeclaration typeDecl, Type type)
            {
                // Add fields as ExpressoSymbols so that we can reference raw value style enum members
                foreach(var field_decl in typeDecl.Members.OfType<FieldDeclaration>()){
                    foreach(var initializer in field_decl.Initializers){
                        var field = type.GetField(field_prefix + initializer.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                        if(field == null)
                            throw new InvalidOperationException(string.Format("{0} is null! Something wrong has occurred!", initializer.Name));

                        UpdateSymbol(initializer.NameToken, new ExpressoSymbol{PropertyOrField = field});
                    }
                }
            }

            Type DetermineType(AstType astType)
            {
                if(self_type != null)
                    return self_type;

                if(astType is ParameterType param_type){
                    var generic_type = generator.generic_types.Where(gt => gt.Name == astType.Name)
                                                .First();
                    return generic_type;
                }

                return CSharpCompilerHelpers.GetNativeType(astType);
            }
        }
    }
}

