//
// TypeSystemConvertWalker.cs
//
// Author:
//       train12 <kotonechan@live.jp>
//
// Copyright (c) 2018 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using Expresso.Ast;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;

namespace Expresso.TypeSystem
{
    internal partial class TypeSystemConvertWalker : IAstWalker<IUnresolvedEntity>
    {
        ExpressoUnresolvedFile unresolved_file;
        DefaultUnresolvedTypeDefinition current_type_def;
        DefaultUnresolvedMethod current_method;
        InterningProvider interning_provider = new SimpleInterningProvider();

        #region Properties
        public ExpressoUnresolvedFile UnresolvedFile => unresolved_file;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Expresso.TypeSystem.TypeSystemConvertWalker"/> class.
        /// </summary>
        /// <param name="fileName">The file name(used for DomRegions).</param>
        public TypeSystemConvertWalker(string fileName)
        {
            if(fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            unresolved_file = new ExpressoUnresolvedFile{
                FileName = fileName
            };
        }

        DomRegion MakeRegion(TextLocation start, TextLocation end)
        {
            return new DomRegion(unresolved_file.FileName, start.Line, start.Column, end.Line, end.Column);
        }

        DomRegion MakeRegion(AstNode node)
        {
            if(node == null || node.IsNull)
                return DomRegion.Empty;
            else
                return MakeRegion(node.StartLocation, node.EndLocation);
        }

        public IUnresolvedEntity VisitAliasDeclaration(AliasDeclaration aliasDecl)
        {
            throw new NotImplementedException();
        }

        public IUnresolvedEntity VisitAssignment(AssignmentExpression assignment)
        {
            assignment.Left.AcceptWalker(this);
            assignment.Right.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitAst(ExpressoAst ast)
        {
            foreach(var import in ast.Imports)
                VisitImportDeclaration(import);

            var type_def = new DefaultUnresolvedTypeDefinition((string)null, ConvertToPascalCase(ast.Name));
            current_type_def = type_def;
            foreach(var decl in ast.Declarations)
                decl.AcceptWalker(this);

            return null;
        }

        public IUnresolvedEntity VisitBinaryExpression(BinaryExpression binaryExpr)
        {
            binaryExpr.Left.AcceptWalker(this);
            binaryExpr.Right.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitBlock(BlockStatement block)
        {
            foreach(var stmt in block.Statements)
                stmt.AcceptWalker(this);

            return null;
        }

        public IUnresolvedEntity VisitBreakStatement(BreakStatement breakStmt)
        {
            VisitLiteralExpression(breakStmt.CountExpression);
            return null;
        }

        public IUnresolvedEntity VisitCallExpression(CallExpression call)
        {
            call.Target.AcceptWalker(this);
            foreach(var arg in call.Arguments)
                arg.AcceptWalker(this);

            return null;
        }

        public IUnresolvedEntity VisitCastExpression(CastExpression castExpr)
        {
            castExpr.Target.AcceptWalker(this);
            castExpr.ToExpression.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitCatchClause(CatchClause catchClause)
        {
            VisitIdentifier(catchClause.Identifier);
            catchClause.Body.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitClosureLiteralExpression(ClosureLiteralExpression closure)
        {
            foreach(var p in closure.Parameters)
                VisitParameterDeclaration(p);

            VisitBlock(closure.Body);
            return null;
        }

        public IUnresolvedEntity VisitCollectionPattern(CollectionPattern collectionPattern)
        {
            foreach(var item in collectionPattern.Items)
                item.AcceptWalker(this);

            return null;
        }

        public IUnresolvedEntity VisitCommentNode(CommentNode comment)
        {
            return null;
        }

        public IUnresolvedEntity VisitComprehensionExpression(ComprehensionExpression comp)
        {
            comp.Item.AcceptWalker(this);
            comp.Body.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitComprehensionForClause(ComprehensionForClause compFor)
        {
            compFor.Left.AcceptWalker(this);
            compFor.Target.AcceptWalker(this);
            compFor.Body.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitComprehensionIfClause(ComprehensionIfClause compIf)
        {
            compIf.Condition.AcceptWalker(this);
            compIf.Body.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitConditionalExpression(ConditionalExpression condExpr)
        {
            condExpr.Condition.AcceptWalker(this);
            condExpr.TrueExpression.AcceptWalker(this);
            condExpr.FalseExpression.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitContinueStatement(ContinueStatement continueStmt)
        {
            VisitLiteralExpression(continueStmt.CountExpression);
            return null;
        }

        public IUnresolvedEntity VisitDestructuringPattern(DestructuringPattern destructuringPattern)
        {
            destructuringPattern.TypePath.AcceptWalker(this);
            foreach(var item in destructuringPattern.Items)
                item.AcceptWalker(this);

            return null;
        }

        public IUnresolvedEntity VisitDoWhileStatement(DoWhileStatement doWhileStmt)
        {
            VisitWhileStatement(doWhileStmt.Delegator);
            return null;
        }

        public IUnresolvedEntity VisitEmptyStatement(EmptyStatement emptyStmt)
        {
            return null;
        }

        public IUnresolvedEntity VisitExpressionPattern(ExpressionPattern exprPattern)
        {
            exprPattern.Expression.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitExpressionStatement(ExpressionStatement exprStmt)
        {
            exprStmt.Expression.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitExpressoTokenNode(ExpressoTokenNode tokenNode)
        {
            return null;
        }

        public IUnresolvedEntity VisitFieldDeclaration(FieldDeclaration fieldDecl)
        {
            bool is_single_field = fieldDecl.Initializers.Count == 1;
            var modifiers = fieldDecl.Modifiers;
            DefaultUnresolvedField field = null;
            foreach(var initializer in fieldDecl.Initializers)
            {
                field = new DefaultUnresolvedField(current_type_def, initializer.Name);

                field.Region = is_single_field ? MakeRegion(fieldDecl) : MakeRegion(initializer);
                field.BodyRegion = MakeRegion(initializer);

                ApplyModifiers(field, modifiers);
                field.IsReadOnly = (modifiers & Modifiers.Immutable) != 0;

                field.ReturnType = ConvertTypeReference(fieldDecl.ReturnType);

                current_type_def.Members.Add(field);
                field.ApplyInterningProvider(interning_provider);
            }

            return is_single_field ? field : null;
        }

        public IUnresolvedEntity VisitFinallyClause(FinallyClause finallyClause)
        {
            VisitBlock(finallyClause.Body);
            return null;
        }

        public IUnresolvedEntity VisitForStatement(ForStatement forStmt)
        {
            forStmt.Left.AcceptWalker(this);
            forStmt.Target.AcceptWalker(this);
            VisitBlock(forStmt.Body);
            return null;
        }

        public IUnresolvedEntity VisitFunctionDeclaration(FunctionDeclaration funcDecl)
        {
            var method = new DefaultUnresolvedMethod(current_type_def, funcDecl.Name);
            current_method = method;    // required for resolving type parameters
            method.Region = MakeRegion(funcDecl);
            method.BodyRegion = MakeRegion(funcDecl.Body);

            //ConvertTypeParameters();

            method.ReturnType = ConvertTypeReference(funcDecl.ReturnType);

            ApplyModifiers(method, funcDecl.Modifiers);
            method.HasBody = true;

            ConvertParameters(method.Parameters, funcDecl.Parameters);

            current_type_def.Members.Add(method);
            current_method = null;
            method.ApplyInterningProvider(interning_provider);
            return method;
        }

        public IUnresolvedEntity VisitFunctionType(FunctionType funcType)
        {
            VisitIdentifier(funcType.IdentifierNode);
            foreach(var p in funcType.Parameters)
                p.AcceptWalker(this);
            
            return null;
        }

        public IUnresolvedEntity VisitIdentifier(Identifier ident)
        {
            return null;
        }

        public IUnresolvedEntity VisitIdentifierPattern(IdentifierPattern identifierPattern)
        {
            VisitIdentifier(identifierPattern.Identifier);
            identifierPattern.InnerPattern.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitIfStatement(IfStatement ifStmt)
        {
            ifStmt.Condition.AcceptWalker(this);
            VisitBlock(ifStmt.TrueBlock);
            ifStmt.FalseStatement.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitIgnoringRestPattern(IgnoringRestPattern restPattern)
        {
            return null;
        }

        public IUnresolvedEntity VisitImportDeclaration(ImportDeclaration importDecl)
        {
            return null;
        }

        public IUnresolvedEntity VisitIndexerExpression(IndexerExpression indexExpr)
        {
            indexExpr.Target.AcceptWalker(this);
            foreach(var arg in indexExpr.Arguments)
                arg.AcceptWalker(this);
            
            return null;
        }

        public IUnresolvedEntity VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq)
        {
            intSeq.Start.AcceptWalker(this);
            intSeq.End.AcceptWalker(this);
            intSeq.Step.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
        {
            keyValue.KeyExpression.AcceptWalker(this);
            keyValue.ValueExpression.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitKeyValuePattern(KeyValuePattern keyValuePattern)
        {
            VisitIdentifier(keyValuePattern.KeyIdentifier);
            keyValuePattern.Value.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitLiteralExpression(LiteralExpression literal)
        {
            return null;
        }

        public IUnresolvedEntity VisitMatchClause(MatchPatternClause matchClause)
        {
            foreach(var pattern in matchClause.Patterns)
                pattern.AcceptWalker(this);

            matchClause.Guard.AcceptWalker(this);
            matchClause.Body.AcceptWalker(this);

            return null;
        }

        public IUnresolvedEntity VisitMatchStatement(MatchStatement matchStmt)
        {
            matchStmt.Target.AcceptWalker(this);
            foreach(var clause in matchStmt.Clauses)
                VisitMatchClause(clause);

            return null;
        }

        public IUnresolvedEntity VisitMemberReference(MemberReferenceExpression memRef)
        {
            memRef.Target.AcceptWalker(this);
            VisitIdentifier(memRef.Member);
            return null;
        }

        public IUnresolvedEntity VisitMemberType(MemberType memberType)
        {
            memberType.Target.AcceptWalker(this);
            VisitSimpleType(memberType.ChildType);
            return null;
        }

        public IUnresolvedEntity VisitNewLine(NewLineNode newlineNode)
        {
            return null;
        }

        public IUnresolvedEntity VisitNullNode(AstNode nullNode)
        {
            return null;
        }

        public IUnresolvedEntity VisitNullReferenceExpression(NullReferenceExpression nullRef)
        {
            return null;
        }

        public IUnresolvedEntity VisitObjectCreationExpression(ObjectCreationExpression creation)
        {
            creation.TypePath.AcceptWalker(this);
            foreach(var item in creation.Items)
                VisitKeyValueLikeExpression(item);

            return null;
        }

        public IUnresolvedEntity VisitParameterDeclaration(ParameterDeclaration parameterDecl)
        {
            VisitIdentifier(parameterDecl.NameToken);
            parameterDecl.Option.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitParameterType(ParameterType paramType)
        {
            VisitIdentifier(paramType.IdentifierToken);
            return null;
        }

        public IUnresolvedEntity VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
        {
            parensExpr.Expression.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitPathExpression(PathExpression pathExpr)
        {
            foreach(var item in pathExpr.Items)
                VisitIdentifier(item);

            return null;
        }

        public IUnresolvedEntity VisitPatternPlaceholder(AstNode placeholder, Pattern child)
        {
            return null;
        }

        public IUnresolvedEntity VisitPatternWithType(PatternWithType pattern)
        {
            pattern.Pattern.AcceptWalker(this);
            pattern.Type.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitPlaceholderType(PlaceholderType placeholderType)
        {
            return null;
        }

        public IUnresolvedEntity VisitPrimitiveType(PrimitiveType primitiveType)
        {
            return null;
        }

        public IUnresolvedEntity VisitReferenceType(ReferenceType referenceType)
        {
            referenceType.BaseType.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitReturnStatement(ReturnStatement returnStmt)
        {
            returnStmt.Expression.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
        {
            return null;
        }

        public IUnresolvedEntity VisitSequenceExpression(SequenceExpression seqExpr)
        {
            foreach(var item in seqExpr.Items)
                item.AcceptWalker(this);

            return null;
        }

        public IUnresolvedEntity VisitSequenceInitializer(SequenceInitializer seqInitializer)
        {
            VisitSimpleType(seqInitializer.ObjectType);
            foreach(var item in seqInitializer.Items)
                item.AcceptWalker(this);

            return null;
        }

        public IUnresolvedEntity VisitSimpleType(SimpleType simpleType)
        {
            VisitIdentifier(simpleType.IdentifierNode);
            foreach(var type_arg in simpleType.TypeArguments)
                type_arg.AcceptWalker(this);

            return null;
        }

        public IUnresolvedEntity VisitSuperReferenceExpression(SuperReferenceExpression superRef)
        {
            return null;
        }

        public IUnresolvedEntity VisitTextNode(TextNode textNode)
        {
            return null;
        }

        public IUnresolvedEntity VisitThrowStatement(ThrowStatement throwStmt)
        {
            VisitObjectCreationExpression(throwStmt.CreationExpression);
            return null;
        }

        public IUnresolvedEntity VisitTryStatement(TryStatement tryStmt)
        {
            VisitBlock(tryStmt.EnclosingBlock);
            foreach(var catch_clause in tryStmt.CatchClauses)
                VisitCatchClause(catch_clause);

            VisitFinallyClause(tryStmt.FinallyClause);
            return null;
        }

        public IUnresolvedEntity VisitTuplePattern(TuplePattern tuplePattern)
        {
            foreach(var pattern in tuplePattern.Patterns)
                pattern.AcceptWalker(this);

            return null;
        }

        public IUnresolvedEntity VisitTypeDeclaration(TypeDeclaration typeDecl)
        {
            return null;
        }

        public IUnresolvedEntity VisitUnaryExpression(UnaryExpression unaryExpr)
        {
            unaryExpr.Operand.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStatment)
        {
            VisitVariableInitializer(valueBindingForStatment.Initializer);
            VisitBlock(valueBindingForStatment.Body);
            return null;
        }

        public IUnresolvedEntity VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
        {
            foreach(var variable in varDecl.Variables)
                VisitVariableInitializer(variable);

            return null;
        }

        public IUnresolvedEntity VisitVariableInitializer(VariableInitializer initializer)
        {
            VisitIdentifier(initializer.NameToken);
            initializer.Initializer.AcceptWalker(this);
            return null;
        }

        public IUnresolvedEntity VisitWhileStatement(WhileStatement whileStmt)
        {
            whileStmt.Condition.AcceptWalker(this);
            VisitBlock(whileStmt.Body);
            return null;
        }

        public IUnresolvedEntity VisitWhitespace(WhitespaceNode whitespaceNode)
        {
            return null;
        }

        public IUnresolvedEntity VisitWildcardPattern(WildcardPattern wildcardPattern)
        {
            return null;
        }

        public IUnresolvedEntity VisitYieldStatement(YieldStatement yieldStmt)
        {
            yieldStmt.Expression.AcceptWalker(this);
            return null;
        }

        #region Types
        ITypeReference ConvertTypeReference(AstType type, NameLookupMode lookupMode = NameLookupMode.Type)
        {
            return type.ToTypeReference(lookupMode, interning_provider);
        }
        #endregion

        #region Modifiers
        static void ApplyModifiers(DefaultResolvedTypeDefinition td, Modifiers modifiers)
        {
            //td.Accessibility = GetAccessibility(modifiers) ?? Accessibility.Private;
            //td.IsAbstract = (modifiers & (Modifiers.Abstract | Modifiers.Static)) != 0;
        }

        static void ApplyModifiers(AbstractUnresolvedMember member, Modifiers modifiers)
        {
            member.Accessibility = GetAccessibility(modifiers) ?? Accessibility.Private;
            member.IsAbstract = (modifiers & Modifiers.Abstract) != 0;
            member.IsOverride = (modifiers & Modifiers.Override) != 0;
            member.IsStatic = (modifiers & Modifiers.Static) != 0;
        }

        static Accessibility? GetAccessibility(Modifiers modifiers)
        {
            switch(modifiers & Modifiers.VisibilityMask){
            case Modifiers.Private:
                return Accessibility.Private;

            case Modifiers.Protected:
                return Accessibility.Protected;

            case Modifiers.Public:
                return Accessibility.Public;

            case Modifiers.Export:
                return Accessibility.Public;

            default:
                return null;
            }
        }
        #endregion

        #region Parameters
        void ConvertParameters(IList<IUnresolvedParameter> outputList, IEnumerable<ParameterDeclaration> parameters)
        {
            foreach(var pd in parameters){
                var p = new DefaultUnresolvedParameter(ConvertTypeReference(pd.ReturnType), interning_provider.Intern(pd.Name));
                p.Region = MakeRegion(pd);
                if(pd.IsVariadic)
                    p.IsParams = true;
                else if(pd.ReturnType is ReferenceType)
                    p.IsRef = true;

                //if(!pd.Option.IsNull)
                //    p.DefaultValue = ConvertConstantValue(p.Type, pd.Option);
                outputList.Add(interning_provider.Intern(p));
            }
        }
        #endregion

        static string ConvertToPascalCase(string identifier)
        {
            return char.ToUpper(identifier[0]) + identifier.Substring(1);
        }
    }
}
