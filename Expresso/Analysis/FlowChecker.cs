using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ICSharpCode.NRefactory.PatternMatching;


/**
 * The data flow.
 * 
 * Each local name is represented as 2 bits:
 * One is for definitive assignment, the other is for uninitialized use detection.
 * The only difference between the two is behavior on delete.
 * On delete, the name is not assigned to meaningful value (we need to check at runtime if it's initialized),
 * but it is not uninitialized either (because delete statement will set it to Uninitialized.instance).
 * This way, codegen doesn't have to emit an explicit initialization for it.
 * 
 * The bit arrays in the flow checker hold the state and upon encountering NameExpr we figure
 * out whether the name has not yet been initialized at all (in which case we need to emit the
 * first explicit assignment to Uninitialized.instance and guard the use with an inlined check
 * or whether it is definitely assigned (we don't need to inline the check)
 * or whether it may be uninitialized, in which case we must only guard the use by inlining the Uninitialized check
 * 
 * More details on the bits.
 * 
 * First bit (variable is assigned a value):
 *  1 .. variable is definitely assigned to a value
 *  0 .. variable is not assigned to a value at this point (it could have been deleted or just not assigned yet)
 * Second bit (variable is assigned a value or is deleted):
 *  1 .. variable is definitely initialized (either by a value or by deletion)
 *  0 .. variable is not initialized at this point (neither assigned nor deleted)
 * 
 * Valid combinations:
 *  11 .. initialized
 *  01 .. deleted
 *  00 .. may not be initialized
 */

namespace Expresso.Ast.Analysis
{
    /// <summary>
    /// The flow checker is responsible for analyzing correctness of flow control of each path
    /// in declarations.
    /// </summary>
    class FlowChecker : IAstWalker
	{
		BitArray bits;
		Stack<BitArray> loops;
        Parser parser;
        SymbolTable symbols;
		
        FlowChecker(Parser parser, ExpressoAst decl)
		{
            bits = new BitArray(symbols.NumberOfSymbols * 2);
            this.parser = parser;
		}
		
		/*[Conditional("DEBUG")]
		public void Dump(BitArray bits)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.AppendFormat("FlowChecker ({0})", scope is FunctionDeclaration ? ((FunctionDeclaration)scope).Name :
			                scope is TypeDefinition ? ((TypeDefinition)scope).Name : "");
			sb.Append('{');
			bool comma = false;
			foreach(var binding in variables){
				if(comma) sb.Append(", ");
				else comma = true;

				int index = 2 * binding.Value.Offset;
				sb.AppendFormat("{0}:{1}{2}",
				                binding.Key,
				                bits.Get(index) ? "*" : "-",
				                bits.Get(index + 1) ? "-" : "*");
				if(binding.Value.ReadBeforeInitialized)
					sb.Append("#");
			}
			sb.Append('}');
			Debug.WriteLine(sb.ToString());
		}*/
		
        void SetAssigned(Identifier variable, bool value)
		{
            bits.Set((int)variable.IdentifierId * 2, value);
		}
		
        void SetInitialized(Identifier variable, bool value)
		{
            bits.Set((int)variable.IdentifierId * 2 + 1, value);
		}

        bool IsAssigned(Identifier variable)
		{
            return bits.Get((int)variable.IdentifierId * 2);
		}
		
        bool IsInitialized(Identifier variable)
		{
            return bits.Get((int)variable.IdentifierId * 2 + 1);
		}
		
        public static void Check(ExpressoAst ast, Parser parser)
		{
            if(parser.Symbols != null){
                FlowChecker fc = new FlowChecker(parser, ast);
                ast.AcceptWalker(fc);
			}
		}
		
		public void Define(string name)
		{
            Identifier variable = symbols.GetSymbol(name);
            if(variable != null){
                SetAssigned(variable, true);
                SetInitialized(variable, true);
			}
		}
		
		/*public void Delete(string name)
		{
			ExpressoVariable binding;
			if(variables.TryGetValue(name, out binding)){
				SetAssigned(binding, false);
				SetInitialized(binding, true);
			}
		}*/
		
		void PushLoop(BitArray ba)
		{
			if(loops == null){
				loops = new Stack<BitArray>();
			}
			loops.Push(ba);
		}
		
		BitArray PeekLoop()
		{
			return loops != null && loops.Count > 0 ? loops.Peek() : null;
		}
		
		void PopLoop()
		{
			if(loops != null)
                loops.Pop();
		}
		
        #region IAstWalker implementation

        public void VisitAst(ExpressoAst ast)
        {
            foreach(var import in ast.Imports)
                import.AcceptWalker(this);

            foreach(var decl in ast.Declarations)
                decl.AcceptWalker(this);
        }

        public void VisitBlock(BlockStatement block)
        {
            foreach(var stmt in block.Statements)
                stmt.AcceptWalker(this);
        }

        public void VisitBreakStatement(BreakStatement breakStmt)
        {
        }

        public void VisitContinueStatement(ContinueStatement continueStmt)
        {
        }

        public void VisitDoWhileStatement(DoWhileStatement doWhileStmt)
        {
            VisitWhileStatement(doWhileStmt.Delegator);
        }

        public void VisitEmptyStatement(EmptyStatement emptyStmt)
        {
        }

        public void VisitExpressionStatement(ExpressionStatement exprStmt)
        {
            exprStmt.Expression.AcceptWalker(this);
        }

        public void VisitForStatement(ForStatement forStmt)
        {
            // Walk the expression
            forStmt.Target.AcceptWalker(this);

            BitArray opte = new BitArray(bits);
            BitArray exit = new BitArray(bits.Length, true);
            PushLoop(exit);

            forStmt.Left.AcceptWalker(this);

            // Walk the body
            forStmt.Body.AcceptWalker(this);

            PopLoop();

            bits.And(exit);

            // Intersect
            bits.And(opte);
        }

        public void VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStmt)
        {
            // Walk the expression
            valueBindingForStmt.Initializer.AcceptWalker(this);

            BitArray opte = new BitArray(bits);
            BitArray exit = new BitArray(bits.Length, true);
            PushLoop(exit);

            // Walk the body
            valueBindingForStmt.Body.AcceptWalker(this);

            PopLoop();

            bits.And(exit);

            // Intersect
            bits.And(opte);
        }

        public void VisitIfStatement(IfStatement ifStmt)
        {
            //BitArray result = new BitArray(bits.Length, true);
            //BitArray save = bits;

            bits = new BitArray(bits.Length);

            /*foreach(IfStatementTest ist in node.Tests) {
                // Set the initial branch value to bits
                _bits.SetAll(false);
                _bits.Or(save);
                
                // Flow the test first
                ist.Test.Walk(this);
                // Flow the body
                ist.Body.Walk(this);
                // Intersect
                result.And(_bits);
            }
            
            // Set the initial branch value to bits
            _bits.SetAll(false);
            _bits.Or(save);
            
            if (node.ElseStatement != null) {
                // Flow the else_
                node.ElseStatement.Walk(this);
            }
            
            // Intersect
            result.And(_bits);
            
            _bits = save;
            
            // Remember the result
            _bits.SetAll(false);
            _bits.Or(result);*/
        }

        public void VisitReturnStatement(ReturnStatement returnStmt)
        {
            returnStmt.Expression.AcceptWalker(this);
        }

        public void VisitMatchStatement(MatchStatement matchStmt)
        {
        }

        public void VisitThrowStatement(ThrowStatement throwStmt)
        {
            
        }

        public void VisitTryStatement(TryStatement tryStmt)
        {
            
        }

        public void VisitWhileStatement(WhileStatement whileStmt)
        {
            // Walk the expression
            whileStmt.Condition.AcceptWalker(this);

            BitArray exit = new BitArray(bits.Length, true);

            PushLoop(exit);
            whileStmt.Body.AcceptWalker(this);
            PopLoop();

            bits.And(exit);
        }

        public void VisitYieldStatement(YieldStatement yieldStmt)
        {
        }

        public void VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
        {
            foreach(var variable in varDecl.Variables)
                variable.AcceptWalker(this);
        }

        public void VisitAssignment(AssignmentExpression assignment)
        {
            //SetAssigned
        }

        public void VisitBinaryExpression(BinaryExpression binaryExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitCallExpression(CallExpression callExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitCastExpression(CastExpression castExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitCatchClause(CatchClause catchClause)
        {
            
        }

        public void VisitClosureLiteralExpression(ClosureLiteralExpression closure)
        {
            
        }

        public void VisitComprehensionExpression(ComprehensionExpression comp)
        {
            BitArray save = bits;
            bits = new BitArray(bits);

            comp.Body.AcceptWalker(this);
            comp.Item.AcceptWalker(this);

            bits = save;
        }

        public void VisitComprehensionForClause(ComprehensionForClause compFor)
        {
            compFor.Body.AcceptWalker(this);
        }

        public void VisitComprehensionIfClause(ComprehensionIfClause compIf)
        {
            compIf.Body.AcceptWalker(this);
        }

        public void VisitConditionalExpression(ConditionalExpression condExpr)
        {
        }

        public void VisitFinallyClause(FinallyClause finallyClause)
        {
            
        }

        public void VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
        {
            throw new NotImplementedException();
        }

        public void VisitLiteralExpression(LiteralExpression literal)
        {
            throw new NotImplementedException();
        }

        public void VisitIdentifier(Identifier ident)
        {
            if(!IsAssigned(ident)){
                parser.ReportSemanticError(
                    "Use of a potentially uninitialized variable {0}",
                    "ES0200",
                    ident,
                    ident.Name
                );
            }
        }

        public void VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq)
        {
            throw new NotImplementedException();
        }

        public void VisitIndexerExpression(IndexerExpression indexExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitMemberReference(MemberReferenceExpression memRef)
        {
            throw new NotImplementedException();
        }

        public void VisitPathExpression(PathExpression pathExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitObjectCreationExpression(ObjectCreationExpression creation)
        {
            throw new NotImplementedException();
        }

        public void VisitSequenceInitializer(SequenceInitializer seqInitializer)
        {
            foreach(var item in seqInitializer.Items)
                item.AcceptWalker(this);
        }

        public void VisitMatchClause(MatchPatternClause matchClause)
        {
            throw new NotImplementedException();
        }

        public void VisitSequenceExpression(SequenceExpression seqExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitUnaryExpression(UnaryExpression unaryExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
        {
            throw new NotImplementedException();
        }

        public void VisitSuperReferenceExpression(SuperReferenceExpression superRef)
        {
            throw new NotImplementedException();
        }

        public void VisitNullReferenceExpression(NullReferenceExpression nullRef)
        {
            throw new NotImplementedException();
        }

        public void VisitCommentNode(CommentNode comment)
        {
            throw new NotImplementedException();
        }

        public void VisitTextNode(TextNode textNode)
        {
            throw new NotImplementedException();
        }

        public void VisitSimpleType(SimpleType simpleType)
        {
            throw new NotImplementedException();
        }

        public void VisitPrimitiveType(PrimitiveType primitiveType)
        {
            throw new NotImplementedException();
        }

        public void VisitReferenceType(ReferenceType referenceType)
        {
            throw new NotImplementedException();
        }

        public void VisitMemberType(MemberType memberType)
        {
            throw new NotImplementedException();
        }

        public void VisitFunctionType(FunctionType funcType)
        {
            throw new NotImplementedException();
        }

        public void VisitParameterType(ParameterType paramType)
        {
            throw new NotImplementedException();
        }

        public void VisitPlaceholderType(PlaceholderType placeholderType)
        {
            throw new NotImplementedException();
        }

        public void VisitAttributeSection(AttributeSection section)
        {
            throw new InvalidOperationException();
        }

        public void VisitImportDeclaration(ImportDeclaration importDecl)
        {
            // An import declaration always introduces new variable(s) into the module scope.
            foreach(var import_path in importDecl.ImportPaths)
                Define(import_path.Name);
        }

        public void VisitFunctionDeclaration(FunctionDeclaration funcDecl)
        {
            /*if(funcDecl == scope){
                // the function body is being analyzed, go deep:
                foreach(ParameterDeclaration p in funcDecl.Parameters)
                    Define(p.Name);
            }else{*/
                // analyze the function definition itself (it is visited while analyzing parent scope):
                Define(funcDecl.Name);
                foreach(ParameterDeclaration p in funcDecl.Parameters){
                    if(p.Option != null)
                        p.Option.AcceptWalker(this);
                } 
            //}
        }

        public void VisitTypeDeclaration(TypeDeclaration typeDecl)
        {
            /*if(scope == typeDecl){
                // the class body is being analyzed, go deep:
                foreach(var member in typeDecl.Members)
                    member.AcceptWalker(this);
            }else{*/
                // analyze the type definition itself (it is visited while analyzing parent scope):
                Define(typeDecl.Name);
                //foreach(Expression e in node.Bases){
                //  e.Walk(this);
                //}
            //}
        }

        public void VisitAliasDeclaration(AliasDeclaration aliasDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitFieldDeclaration(FieldDeclaration fieldDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitParameterDeclaration(ParameterDeclaration parameterDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitVariableInitializer(VariableInitializer initializer)
        {
            if(!initializer.Initializer.IsNull){
                if(initializer.Pattern is PatternWithType pattern)
                    MakePatternSet(pattern.Pattern);
                else
                    MakePatternSet(initializer.Pattern);
            }
        }

        public void VisitWildcardPattern(WildcardPattern wildcardPattern)
        {
            throw new NotImplementedException();
        }

        public void VisitIdentifierPattern(IdentifierPattern identifierPattern)
        {
            throw new NotImplementedException();
        }

        public void VisitCollectionPattern(CollectionPattern collectionPattern)
        {
            throw new NotImplementedException();
        }

        public void VisitDestructuringPattern(DestructuringPattern destructuringPattern)
        {
            throw new NotImplementedException();
        }

        public void VisitTuplePattern(TuplePattern tuplePattern)
        {
            throw new NotImplementedException();
        }

        public void VisitExpressionPattern(ExpressionPattern exprPattern)
        {
            throw new NotImplementedException();
        }

        public void VisitIgnoringRestPattern(IgnoringRestPattern restPattern)
        {
            throw new NotImplementedException();
        }

        public void VisitKeyValuePattern(KeyValuePattern keyValuePattern)
        {
            throw new NotImplementedException();
        }

        public void VisitPatternWithType(PatternWithType pattern)
        {
            throw new NotImplementedException();
        }

        public void VisitNullNode(AstNode nullNode)
        {
            throw new NotImplementedException();
        }

        public void VisitNewLine(NewLineNode newlineNode)
        {
            throw new NotImplementedException();
        }

        public void VisitWhitespace(WhitespaceNode whitespaceNode)
        {
            throw new NotImplementedException();
        }

        public void VisitExpressoTokenNode(ExpressoTokenNode tokenNode)
        {
            throw new NotImplementedException();
        }

        public void VisitPatternPlaceholder(AstNode placeholder, Pattern child)
        {
            throw new NotImplementedException();
        }

        #endregion

        void MakePatternSet(PatternConstruct pattern)
        {
            if(pattern is IdentifierPattern ident_pat){
                SetAssigned(ident_pat.Identifier, true);
                SetInitialized(ident_pat.Identifier, true);
            }else if(pattern is TuplePattern tuple_pat){
                foreach(var pat in tuple_pat.Patterns){
                    if(pat is IdentifierPattern ident_pat2){
                        SetAssigned(ident_pat2.Identifier, true);
                        SetInitialized(ident_pat2.Identifier, true);
                    }
                }
            }
        }
	}
}

