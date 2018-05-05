using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using Expresso.Ast;
using Expresso.Ast.Analysis;
using Expresso.Runtime.Builtins;

namespace Expresso.CodeGen
{
    using CSharpExpr = System.Linq.Expressions.Expression;
    using ExprTree = System.Linq.Expressions;

    /// <summary>
    /// An <see cref="IComparer&lt;T&gt;"/> implementation that sorts import paths.
    /// With it, paths to the standard library come first and others come second.
    /// </summary>
    class ImportPathComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if(x.StartsWith("System.", StringComparison.CurrentCulture) && !y.StartsWith("System.", StringComparison.CurrentCulture))
                return -1;
            else if(x.StartsWith("System.", StringComparison.CurrentCulture) && y.StartsWith("System.", StringComparison.CurrentCulture))
                return 0;
            else if(!x.StartsWith("System.", StringComparison.CurrentCulture) && y.StartsWith("System.", StringComparison.CurrentCulture))
                return -1;
            else
                return 0;
        }
    }

    /// <summary>
    /// Expressoの構文木を解釈してC#の式木にコンパイルするクラス。
    /// It emitts C#'s expression tree nodes from the AST of Expresso.
    /// Assumes we are all OK with syntax and semantics since this class is considered
    /// to belong to the backend part in compiler theory.
    /// </summary>
    /// <remarks>
    /// While emitting, we don't check the semantics and validity because the type check and
    /// other semantics analisys phases do that.
    /// We just care if an Expresso's expresson is convertible and has been converted to C# expressions or not.
    /// </remarks>
    public partial class CSharpEmitter : IAstWalker<CSharpEmitterContext, CSharpExpr>
	{
        //###################################################
        //# Symbols defined in the whole program.
        //# It is represented as a hash table rather than a symbol table
        //# because we have defined a symbol id on all the identifier nodes
        //# that identifies the symbol uniqueness within the whole program.
        //###################################################
        // FIXME: make it accessible only from the tester class
        public static Dictionary<uint, ExpressoSymbol> Symbols = new Dictionary<uint, ExpressoSymbol>();
        static int LoopCounter = 1, ClosureId = 0;
        static ExprTree.LabelTarget ReturnTarget = null;
        static CSharpExpr DefaultReturnValue = null;
        static int VariableCount = 0;
		
        const string ClosureMethodName = "__Apply";
        const string ClosureDelegateName = "__ApplyMethod";
        bool has_continue;

        Parser parser;
        SymbolTable symbol_table;
        ExpressoCompilerOptions options;
        MatchClauseIdentifierDefiner identifier_definer;
        ItemTypeInferencer item_type_inferencer;

        List<ExprTree.LabelTarget> break_targets = new List<ExprTree.LabelTarget>();
        List<ExprTree.LabelTarget> continue_targets = new List<ExprTree.LabelTarget>();

        int scope_counter = 0;

        ExprTree.SymbolDocumentInfo document_info;

        /// <summary>
        /// Gets the generated assembly.
        /// </summary>
        public AssemblyBuilder AssemblyBuilder{
            get; private set;
        }

        public CSharpEmitter(Parser parser, ExpressoCompilerOptions options)
        {
            this.parser = parser;
            symbol_table = parser.Symbols;
            this.options = options;
            identifier_definer = new MatchClauseIdentifierDefiner();
            item_type_inferencer = new ItemTypeInferencer(this);

            CSharpCompilerHelpers.AddPrimitiveNativeSymbols();
        }

        static Tuple<CSharpExpr, ExprTree.ParameterExpression> MakeEnumeratorCreations(CSharpExpr iterator)
        {
            ExprTree.ParameterExpression param;
            CSharpExpr creation;

            if(iterator.Type.Name.Contains("Dictionary")){
                var type = typeof(DictionaryEnumerator<,>);
                var iterator_type = iterator.Type;
                var substituted_type = type.MakeGenericType(iterator_type.GetGenericArguments());
                var ctor = substituted_type.GetConstructors().First();

                param = CSharpExpr.Parameter(substituted_type, "__iter");
                creation = CSharpExpr.Assign(param, CSharpExpr.New(ctor, iterator));
            }else{
                Type enumerator_type;
                if(iterator.Type.Name.StartsWith("ExpressoIntegerSequence", StringComparison.CurrentCulture) || iterator.Type.Name.StartsWith("Slice", StringComparison.CurrentCulture))
                    enumerator_type = iterator.Type;
                else
                    enumerator_type = typeof(IEnumerable<>).MakeGenericType(iterator.Type.IsArray ? iterator.Type.GetElementType() : iterator.Type.GenericTypeArguments.First());
                
                var get_enumerator_method = enumerator_type.GetMethod("GetEnumerator");
                param = CSharpExpr.Parameter(get_enumerator_method.ReturnType, "__iter");

                creation = CSharpExpr.Assign(param, CSharpExpr.Call(iterator, get_enumerator_method));
            }

            return new Tuple<CSharpExpr, ExprTree.ParameterExpression>(creation, param);
        }

        static IEnumerable<CSharpExpr> MakeIterableAssignments(IEnumerable<ExprTree.ParameterExpression> variables, ExprTree.ParameterExpression enumerator,
            ExprTree.LabelTarget breakTarget)
        {
            var contents = new List<CSharpExpr>();
            var iterator_type = enumerator.Type;
            var move_method = iterator_type.GetInterface("IEnumerator").GetMethod("MoveNext");
            var move_call = CSharpExpr.Call(enumerator, move_method);
            var check_failure = CSharpExpr.IfThen(CSharpExpr.IsFalse(move_call), CSharpExpr.Goto(breakTarget));
            contents.Add(check_failure);

            var current_property = iterator_type.GetProperty("Current");
            if(variables.Count() == 1){
                contents.Add(CSharpExpr.Assign(variables.First(), CSharpExpr.Property(enumerator, current_property)));
            }else{
                var current_access = CSharpExpr.Property(enumerator, current_property);
                var tuple_type = current_property.PropertyType;
                if(!tuple_type.Name.Contains("Tuple"))
                    throw new InvalidOperationException("iterators must return a tuple type when assigned to a tuple pattern");
                
                foreach(var pair in Enumerable.Range(1, variables.Count() + 1).Zip(variables, (l, r) => new Tuple<int, ExprTree.ParameterExpression>(l, r))){
                    var item_property = tuple_type.GetProperty("Item" + pair.Item1);
                    var item_access = CSharpExpr.Property(current_access, item_property);
                    contents.Add(CSharpExpr.Assign(pair.Item2, item_access));
                }
            }

            return contents;
        }

        internal static void AddSymbol(Identifier ident, ExpressoSymbol symbol)
        {
            if(ident.IdentifierId == 0)
                throw new InvalidOperationException("An invalid identifier is invalid because it can't be used for any purpose");
            
            try{
                Symbols.Add(ident.IdentifierId, symbol);
            }
            catch(ArgumentException){
                throw new InvalidOperationException(string.Format("The native symbol for '{0}' @ <id: {1}> is already added", ident.Name, ident.IdentifierId));
            }
        }

        void DescendScope()
        {
            symbol_table = symbol_table.Children[scope_counter];
        }

        void AscendScope()
        {
            symbol_table = symbol_table.Parent;
        }

        static bool CanReturn(ReturnStatement returnStmt)
        {
            var surrounding_func = returnStmt.Ancestors
                                             .OfType<BlockStatement>()
                                             .First()
                                             .Parent as FunctionDeclaration;
            return surrounding_func == null || surrounding_func != null && surrounding_func.Name != "main";
        }

        static BuildType PersistBuildType(BuildType previous, BuildType newConfig)
        {
            var new_build_config = newConfig;
            new_build_config |= previous.HasFlag(BuildType.Debug) ? BuildType.Debug : BuildType.Release;
            return new_build_config;
        }

        ExpressoSymbol GetRuntimeSymbol(Identifier ident)
        {
            ExpressoSymbol symbol;
            if(Symbols.TryGetValue(ident.IdentifierId, out symbol))
                return symbol;
            else
                return null;
        }

        string GetAssemblyFilePath(ExpressoAst ast)
        {
            return options.BuildType.HasFlag(BuildType.Assembly) ? Path.Combine(options.OutputPath, ast.Name + ".dll") :
                          Path.Combine(options.OutputPath, options.ExecutableName + ".exe");
        }

        string GetPdbFilePath(ExpressoAst ast)
        {
            return options.BuildType.HasFlag(BuildType.Assembly) ? Path.Combine(options.OutputPath, ast.Name + ".pdb") :
                          Path.Combine(options.OutputPath, options.ExecutableName + ".pdb");
        }

        /*void EmitPdb(ExpressoAst ast)
        {
            var pdb_path = GetPdbFilePath(ast);
            Console.WriteLine("Emitting .pdb file at {0}...", pdb_path);
            var writer_provider = new Mono.Cecil.Cil.PortablePdbWriterProvider();
            var asm_resolver = new Mono.Cecil.DefaultAssemblyResolver();
            asm_resolver.AddSearchDirectory(options.OutputPath);

            var asm = Mono.Cecil.AssemblyDefinition.ReadAssembly(
                GetAssemblyFilePath(ast), new Mono.Cecil.ReaderParameters{
                    AssemblyResolver = asm_resolver
                }
            );

            asm.Write(GetAssemblyFilePath(ast), new Mono.Cecil.WriterParameters{
                WriteSymbols = true,
                SymbolWriterProvider = writer_provider
            });
        }*/

        #region IAstWalker implementation

        public CSharpExpr VisitAst(ExpressoAst ast, CSharpEmitterContext context)
        {
            if(context == null)
                context = new CSharpEmitterContext();
            
            var assembly_name = options.BuildType.HasFlag(BuildType.Assembly) ? ast.Name : options.ExecutableName;
            var name = new AssemblyName(assembly_name);

            var asm_builder = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave, options.OutputPath);
            var file_name = options.BuildType.HasFlag(BuildType.Assembly) ? assembly_name + ".dll" : assembly_name + ".exe";

            if(options.BuildType.HasFlag(BuildType.Debug)){
                var attrs = DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations;
                var attr_arg_types = new []{typeof(DebuggableAttribute.DebuggingModes)};
                var attr_arg_values = new object[]{attrs};
                asm_builder.SetCustomAttribute(new CustomAttributeBuilder(
                    typeof(DebuggableAttribute).GetConstructor(attr_arg_types), attr_arg_values
                ));
            }

            var mod_builder = asm_builder.DefineDynamicModule(assembly_name, file_name, options.BuildType.HasFlag(BuildType.Debug));
            //var doc = mod_builder.DefineDocument(Path.GetFileName(parser.scanner.FilePath), LanguageGuid, Guid.Empty, Guid.Empty);

            // Leave the ast.Name as is because otherwise we can't refer to it later when visiting ImportDeclarations
            var type_builder = new LazyTypeBuilder(mod_builder, CSharpCompilerHelpers.ConvertToPascalCase(ast.Name), TypeAttributes.Class | TypeAttributes.Public, Enumerable.Empty<Type>(), true);
            var local_parser = parser;
            if(ast.ExternalModules.Any()){
                context.CurrentModuleCount = ast.ExternalModules.Count;
            }else{
                Debug.Assert(symbol_table.Name == "programRoot", "The symbol_table should indicate 'programRoot' before generating codes.");
                if(context.CurrentModuleCount > 0){
                    parser = parser.InnerParsers[parser.InnerParsers.Count - context.CurrentModuleCount--];
                    symbol_table = parser.Symbols;

                    local_parser = parser;
                }
            }

            context.AssemblyBuilder = asm_builder;
            context.ModuleBuilder = mod_builder;
            context.LazyTypeBuilder = type_builder;

            if(ast.Name == "main")
                options.BuildType = PersistBuildType(options.BuildType, BuildType.Assembly);

            context.ExternalModuleType = null;
            ast.Imports.OrderBy(i => i.ImportPaths.First().Name, new ImportPathComparer());
            foreach(var pair in ast.ExternalModules.Zip(ast.Imports.SkipWhile(i => i.ImportPaths.First().Name.StartsWith("System.", StringComparison.CurrentCulture)),
                                                        (l, r) => new Tuple<ExpressoAst, ImportDeclaration>(l, r))){
                Debug.Assert(pair.Item2.ImportPaths.First().Name.StartsWith(pair.Item1.Name, StringComparison.CurrentCulture), "The module name must be matched to the import path");

                var external_module_count = context.CurrentModuleCount;
                var tmp_counter = scope_counter;
                VisitAst(pair.Item1, context);
                context.CurrentModuleCount = external_module_count;
                scope_counter = tmp_counter;

                var first_import = pair.Item2.ImportPaths.First();
                if(!first_import.Name.Contains("::") && !first_import.Name.Contains(".")){
                    Symbols.Add(pair.Item2.ImportPaths.First().IdentifierId, new ExpressoSymbol{
                        Type = context.ExternalModuleType
                    });
                    context.ExternalModuleType = null;
                }
            }
            if(ast.Name == "main")
                options.BuildType = PersistBuildType(options.BuildType, BuildType.Executable);

            context.AssemblyBuilder = asm_builder;
            context.ModuleBuilder = mod_builder;
            context.LazyTypeBuilder = type_builder;
            if(local_parser != null){
                parser = local_parser;
                symbol_table = parser.Symbols;
            }

            document_info = CSharpExpr.SymbolDocument(parser.scanner.FilePath, ExpressoCompilerHelpers.LanguageGuid);

            Console.WriteLine("Emitting code in {0}...", ast.ModuleName);
            foreach(var import in ast.Imports)
                import.AcceptWalker(this, context);

            foreach(var decl in ast.Declarations){
                // Define only the function signatures here so that these functions or methods can call themselves
                DefineFunctionSignaturesAndFields(ast.Declarations, decl, context);
                decl.AcceptWalker(this, context);
            }

            context.ExternalModuleType = type_builder.CreateType();
            asm_builder.Save(file_name);

            //if(options.BuildType.HasFlag(BuildType.Debug))
            //    EmitPdb(ast);

            AssemblyBuilder = asm_builder;

            return null;
        }

        public CSharpExpr VisitBlock(BlockStatement block, CSharpEmitterContext context)
        {
            if(block.IsNull)
                return null;
            
            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            var parent_block = context.ContextAst;
            context.ContextAst = block;

            var contents = new List<CSharpExpr>();
            foreach(var stmt in block.Statements){
                var tmp = stmt.AcceptWalker(this, context);
                if(tmp == null){
                    contents.AddRange(context.Additionals.Cast<CSharpExpr>());
                    context.Additionals.Clear();
                }else{
                    contents.Add(tmp);
                }
            }

            context.ContextAst = parent_block;

            var variables = ConvertSymbolsToParameters().ToList();
            if(block.Parent is CatchClause){
                var catch_clause = (CatchClause)block.Parent;
                var identifier = catch_clause.Identifier;
                variables = variables.Where(v => v.Name != identifier.Name)
                                     .Select(v => v)
                                     .ToList();
            }
            if(context.ContextAst is FunctionDeclaration || context.ContextClosureLiteral != null && block.Parent is ClosureLiteralExpression)
                contents.Add(CSharpExpr.Label(ReturnTarget, DefaultReturnValue));

            AscendScope();
            scope_counter = tmp_counter + 1;
            return CSharpExpr.Block(contents.Last().Type, variables, contents);
        }

        public CSharpExpr VisitBreakStatement(BreakStatement breakStmt, CSharpEmitterContext context)
        {
            int count = (int)breakStmt.CountExpression.Value;
            if(count > break_targets.Count)
                throw new EmitterException("Can not break out of loops that many times!");

            //break upto Count; => goto label;
            return CSharpExpr.Break(break_targets[break_targets.Count - count]);
        }

        public CSharpExpr VisitContinueStatement(ContinueStatement continueStmt, CSharpEmitterContext context)
        {
            int count = (int)continueStmt.CountExpression.Value;
            if(count > continue_targets.Count)
                throw new EmitterException("Can not break out of loops that many times!");

            //continue upto Count; => goto label;
            return CSharpExpr.Continue(continue_targets[continue_targets.Count - count]);
        }

        public CSharpExpr VisitDoWhileStatement(DoWhileStatement doWhileStmt, CSharpEmitterContext context)
        {
            var previous_additionals = context.Additionals;
            context.Additionals = new List<object>();
            var while_stmt = VisitWhileStatement(doWhileStmt.Delegator, context);
            var body = context.Additionals.First() as CSharpExpr;
            context.Additionals = previous_additionals;
                                                            //{ body;
            return CSharpExpr.Block(body, while_stmt);      //  while_stmt}
        }

        public CSharpExpr VisitEmptyStatement(EmptyStatement emptyStmt, CSharpEmitterContext context)
        {
            return CSharpExpr.Empty();
        }

        public CSharpExpr VisitExpressionStatement(ExpressionStatement exprStmt, CSharpEmitterContext context)
        {
            return exprStmt.Expression.AcceptWalker(this, context);
        }

        public CSharpExpr VisitForStatement(ForStatement forStmt, CSharpEmitterContext context)
        {
            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            forStmt.Left.AcceptWalker(this, context);
            var iterator = forStmt.Target.AcceptWalker(this, context);

            var break_target = CSharpExpr.Label("__EndFor" + LoopCounter.ToString());
            var continue_target = CSharpExpr.Label("__StartFor" + LoopCounter.ToString());
            ++LoopCounter;
            break_targets.Add(break_target);
            continue_targets.Add(continue_target);

            // Here, `Body` represents just the body block itself
            // In a for statement, we must move the iterator a step forward
            // and assign the result to inner-scope variables
            var real_body = (ExprTree.BlockExpression)VisitBlock(forStmt.Body, context);

            var variables = ConvertSymbolsToParameters();
            var tmp = MakeEnumeratorCreations(iterator);
            var creation = tmp.Item1;
            var enumerator = tmp.Item2;
            var assignments = MakeIterableAssignments(variables, enumerator, break_target);

            var body = CSharpExpr.Block(variables,
                assignments.Concat(real_body.Expressions)
            );
            var loop = CSharpExpr.Loop(body, break_target, continue_target);
            break_targets.RemoveAt(break_targets.Count - 1);
            continue_targets.RemoveAt(continue_targets.Count - 1);
            AscendScope();
            scope_counter = tmp_counter + 1;

            var contents = new List<CSharpExpr>();
            contents.Add(creation);
            contents.Add(loop);
            return CSharpExpr.Block(new []{enumerator}, contents);
        }

        public CSharpExpr VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStatement, CSharpEmitterContext context)
        {
            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            var parent_params = context.AdditionalParameters;
            context.AdditionalParameters = new List<ExprTree.ParameterExpression>();
            valueBindingForStatement.Initializer.Pattern.AcceptWalker(this, context);
            var bound_variables = context.AdditionalParameters;
            context.AdditionalParameters = parent_params;
            /*foreach(var variable in valueBindingForStatement.Pattern){
                var bound_variable = CSharpExpr.Variable(CSharpCompilerHelper.GetNativeType(variable.NameToken.Type), variable.Name);
                AddSymbol(variable.NameToken, new ExpressoSymbol{Parameter = bound_variable});
                bound_variables.Add(bound_variable);
                iterators.Add(variable.Initializer.AcceptWalker(this, context));
            }*/
            var iterator = valueBindingForStatement.Initializer.Initializer.AcceptWalker(this, context);

            var break_target = CSharpExpr.Label("__EndFor" + LoopCounter.ToString());
            var continue_target = CSharpExpr.Label("__StartFor" + LoopCounter.ToString());
            ++LoopCounter;
            break_targets.Add(break_target);
            continue_targets.Add(continue_target);

            // Here, `body` represents just the body block itself
            // In a for statement, we must move the iterator a step forward
            // and assign the result to inner-scope variables
            var real_body = (ExprTree.BlockExpression)VisitBlock(valueBindingForStatement.Body, context);

            var tmp = MakeEnumeratorCreations(iterator);
            var creation = tmp.Item1;
            var enumerator = tmp.Item2;
            var assignments = MakeIterableAssignments(bound_variables, enumerator, break_target);
            var parameters = ConvertSymbolsToParameters();

            var body = CSharpExpr.Block(parameters.Concat(real_body.Variables),
                assignments.Concat(real_body.Expressions)
            );
            var loop = CSharpExpr.Loop(body, break_target, continue_target);
            break_targets.RemoveAt(break_targets.Count - 1);
            continue_targets.RemoveAt(continue_targets.Count - 1);
            AscendScope();
            scope_counter = tmp_counter + 1;

            var contents = new List<CSharpExpr>();
            contents.Add(creation);
            contents.Add(loop);
            return CSharpExpr.Block(new []{enumerator}, contents);
            // for let x in some_sequence {} => (enumerator){ creation (loop_variables, block_variables){ loop_variable_initializer { real_body } } }
        }

        public CSharpExpr VisitIfStatement(IfStatement ifStmt, CSharpEmitterContext context)
        {
            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            var cond = ifStmt.Condition.AcceptWalker(this, context);
            var true_block = ifStmt.TrueBlock.AcceptWalker(this, context);

            if(ifStmt.FalseStatement.IsNull){
                AscendScope();
                scope_counter = tmp_counter + 1;
                return CSharpExpr.IfThen(cond, true_block);
            }else{
                var false_block = ifStmt.FalseStatement.AcceptWalker(this, context);
                AscendScope();
                scope_counter = tmp_counter + 1;
                return CSharpExpr.IfThenElse(cond, true_block, false_block);
            }
        }

        public CSharpExpr VisitReturnStatement(ReturnStatement returnStmt, CSharpEmitterContext context)
        {
            // If we are in the main function, then make return statements do nothing
            if(!CanReturn(returnStmt))
                return CSharpExpr.Empty();

            var expr = returnStmt.Expression.AcceptWalker(this, context);
            return CSharpExpr.Return(ReturnTarget, expr);
        }

        public CSharpExpr VisitMatchStatement(MatchStatement matchStmt, CSharpEmitterContext context)
        {
            // Match statement semantics: First we evaluate the target expression
            // and assign the result to a temporary variable that's alive within the whole statement.
            // All the pattern clauses must meet the same condition.
            // If context.ContextExpression is an ExprTree.ConditionalExpression
            // we know that we're at least at the second branch.
            // If it is null, then we're at the first branch so just set it to the context expression.
            var target = matchStmt.Target.AcceptWalker(this, context);
            var target_var = CSharpExpr.Parameter(target.Type, "__match_target");

            var block_contents = ExpandCollection(target.Type, target_var, out var parameters);
            context.TemporaryVariable = target_var;
            context.TemporaryExpression = target_var;
            context.ContextExpression = null;
            var context_ast = context.ContextAst;
            context.ContextAst = matchStmt;

            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            //ExprTree.ConditionalExpression res = null, tmp_res = res;
            var exprs = new List<ExprTree.ConditionalExpression>(matchStmt.Clauses.Count);
            foreach(var clause in matchStmt.Clauses){
                var tmp = clause.AcceptWalker(this, context) as ExprTree.ConditionalExpression;
                /*if(tmp_res == null){
                    tmp_res = tmp;
                }else if(tmp == null){
                    tmp_res = CSharpExpr.IfThenElse(tmp_res.Test, tmp_res.IfTrue, context.ContextExpression);
                }else{
                    tmp_res = CSharpExpr.IfThenElse(tmp_res.Test, tmp_res.IfTrue, tmp);
                    if(res == null)
                        res = tmp_res;
                    
                    tmp_res = (ExprTree.ConditionalExpression)tmp_res.IfFalse;
                }*/
                exprs.Add(tmp);
                
                /*if(context.ContextExpression != null){
                    var cond_expr = (ExprTree.ConditionalExpression)context.ContextExpression;
                    cond_expr.Update(cond_expr.Test, cond_expr.IfTrue, tmp);
                    context.ContextExpression = tmp;
                }else{
                    context.ContextExpression = tmp;
                }*/
            }

            var res = ConstructConditionalExpressions(exprs, 0);

            var target_var_assignment = CSharpExpr.Assign(target_var, target);
            block_contents.Insert(0, target_var_assignment);
            block_contents.Add(res);
            parameters.Add(target_var);

            AscendScope();
            scope_counter = tmp_counter + 1;

            context.TemporaryVariable = null;
            context.ContextAst = context_ast;

            return CSharpExpr.Block(parameters, block_contents);
        }

        public CSharpExpr VisitThrowStatement(ThrowStatement throwStmt, CSharpEmitterContext context)
        {
            var thrown_value = VisitObjectCreationExpression(throwStmt.CreationExpression, context);
            return CSharpExpr.Throw(thrown_value);
        }

        public CSharpExpr VisitTryStatement(TryStatement tryStmt, CSharpEmitterContext context)
        {
            var body_block = VisitBlock(tryStmt.EnclosingBlock, context);
            var catches = new List<ExprTree.CatchBlock>();
            foreach(var @catch in tryStmt.CatchClauses){
                VisitCatchClause(@catch, context);
                catches.Add(context.CatchBlock);
            }

            var finally_clause = tryStmt.FinallyClause.AcceptWalker(this, context);
            if(finally_clause == null)
                return CSharpExpr.TryCatch(body_block, catches.ToArray());
            else if(!catches.Any())
                return CSharpExpr.TryFinally(body_block, finally_clause);
            else
                return CSharpExpr.TryCatchFinally(body_block, finally_clause, catches.ToArray());
        }

        public CSharpExpr VisitWhileStatement(WhileStatement whileStmt, CSharpEmitterContext context)
        {
            has_continue = false;

            var end_loop = CSharpExpr.Label("__EndWhile" + LoopCounter.ToString());
            var continue_loop = CSharpExpr.Label("__BeginWhile" + LoopCounter.ToString());
            ++LoopCounter;
            break_targets.Add(end_loop);
            continue_targets.Add(continue_loop);

            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            var condition = CSharpExpr.IfThen(CSharpExpr.Not(whileStmt.Condition.AcceptWalker(this, context)),
                CSharpExpr.Break(end_loop));
            var real_body = VisitBlock(whileStmt.Body, context);
            var body = CSharpExpr.Block(
                condition,
                real_body
            );
            if(context.Additionals != null)
                context.Additionals.Add(real_body);
            
            break_targets.RemoveAt(break_targets.Count - 1);
            continue_targets.RemoveAt(continue_targets.Count - 1);

            AscendScope();
            scope_counter = tmp_counter + 1;

            return has_continue ? CSharpExpr.Loop(body, end_loop, continue_loop) :
                CSharpExpr.Loop(body, end_loop);        //while(condition){body...}
        }

        public CSharpExpr VisitYieldStatement(YieldStatement yieldStmt, CSharpEmitterContext context)
        {
            throw new NotImplementedException();
        }

        public CSharpExpr VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl, CSharpEmitterContext context)
        {
            context.ContextAst = varDecl;
            foreach(var variable in varDecl.Variables){
                // We have to care for only context.Additionals
                variable.AcceptWalker(this, context);
            }
            context.ContextAst = null;
            // A variable declaration statement may contain multiple declarations
            return null;
        }

        public CSharpExpr VisitAssignment(AssignmentExpression assignment, CSharpEmitterContext context)
        {
            var lhs = assignment.Left as SequenceExpression;
            if(lhs != null){    // see if `assignment` is a simultaneous assignment
                // expected form: a, b, ... = 1, 2, ...
                // evaluation order: evaluate expressions from left to right on the right-hand-side
                // and then assign the results on the left-hand-side
                var rhs = assignment.Right as SequenceExpression;
                if(rhs == null)
                    throw new EmitterException("Expected a sequence expression!");

                if(rhs.Items.Count == 1 && assignment.Operator != OperatorType.Power){
                    // if there is just one item on each side
                    // skip making temporary variables
                    switch(assignment.Operator){
                    case OperatorType.Assign:
                        return CSharpExpr.Assign(lhs.Items.First().AcceptWalker(this, context), rhs.Items.First().AcceptWalker(this, context));

                    case OperatorType.Plus:
                        return CSharpExpr.AddAssign(lhs.Items.First().AcceptWalker(this, context), rhs.Items.First().AcceptWalker(this, context));

                    case OperatorType.Minus:
                        return CSharpExpr.SubtractAssign(lhs.Items.First().AcceptWalker(this, context), rhs.Items.First().AcceptWalker(this, context));

                    case OperatorType.Times:
                        return CSharpExpr.MultiplyAssign(lhs.Items.First().AcceptWalker(this, context), rhs.Items.First().AcceptWalker(this, context));

                    case OperatorType.Divide:
                        return CSharpExpr.DivideAssign(lhs.Items.First().AcceptWalker(this, context), rhs.Items.First().AcceptWalker(this, context));

                    case OperatorType.Modulus:
                        return CSharpExpr.ModuloAssign(lhs.Items.First().AcceptWalker(this, context), rhs.Items.First().AcceptWalker(this, context));

                    case OperatorType.Power:
                        var rhs_result = rhs.Items.First().AcceptWalker(this, context);
                        var prev_type = rhs_result.Type;
                        rhs_result = CSharpExpr.Convert(rhs_result, typeof(double));
                        return CSharpExpr.PowerAssign(lhs.Items.First().AcceptWalker(this, context), rhs_result);

                    default:
                        return null;
                    }
                }else{
                    var assignments = new List<CSharpExpr>();
                    var tmp_variables = new List<ExprTree.ParameterExpression>();
                    foreach(var pair in lhs.Items.Zip(rhs.Items,
                        (l, r) => new Tuple<Expression, Expression>(l, r))){
                        //make temporary variables to keep the rhs's results aside
                        //scope: until they are assigned
                        var lhs_result = pair.Item1.AcceptWalker(this, context);
                        var rhs_result = pair.Item2.AcceptWalker(this, context);
                        var param = CSharpExpr.Parameter(rhs_result.Type);
                        tmp_variables.Add(param);

                        switch(assignment.Operator){
                        case OperatorType.Assign:
                            assignments.Add(CSharpExpr.Assign(param, rhs_result));
                            break;

                        case OperatorType.Plus:
                            assignments.Add(CSharpExpr.Assign(param, CSharpExpr.Add(lhs_result, rhs_result)));
                            break;

                        case OperatorType.Minus:
                            assignments.Add(CSharpExpr.Assign(param, CSharpExpr.Subtract(lhs_result, rhs_result)));
                            break;

                        case OperatorType.Times:
                            assignments.Add(CSharpExpr.Assign(param, CSharpExpr.Multiply(lhs_result, rhs_result)));
                            break;

                        case OperatorType.Divide:
                            assignments.Add(CSharpExpr.Assign(param, CSharpExpr.Divide(lhs_result, rhs_result)));
                            break;

                        case OperatorType.Modulus:
                            assignments.Add(CSharpExpr.Assign(param, CSharpExpr.Modulo(lhs_result, rhs_result)));
                            break;

                        case OperatorType.Power:
                            var prev_type = rhs_result.Type;
                            lhs_result = CSharpExpr.Convert(lhs_result, typeof(double));
                            rhs_result = CSharpExpr.Convert(rhs_result, typeof(double));
                            assignments.Add(CSharpExpr.Assign(param, CSharpExpr.Convert(CSharpExpr.Power(lhs_result, rhs_result), prev_type)));
                            break;
                        }
                    }

                    foreach(var pair2 in lhs.Items.Zip(tmp_variables,
                        (l, t) => new Tuple<Expression, ExprTree.Expression>(l, t))){
                        var lhs_expr = pair2.Item1.AcceptWalker(this, context);
                        assignments.Add(CSharpExpr.Assign(lhs_expr, pair2.Item2));
                    }

                    return CSharpExpr.Block(tmp_variables, assignments);
                }
            }else{
                // falls into composition branch
                // expected form: a = b = ...
                var right = assignment.Right.AcceptWalker(this, context);
                var left = assignment.Left.AcceptWalker(this, context);
                return CSharpExpr.Assign(left, right);
            }
        }

        public CSharpExpr VisitBinaryExpression(BinaryExpression binaryExpr, CSharpEmitterContext context)
        {
            var lhs = binaryExpr.Left.AcceptWalker(this, context);
            var rhs = binaryExpr.Right.AcceptWalker(this, context);
            return ConstructBinaryOp(lhs, rhs, binaryExpr.Operator);
        }

        public CSharpExpr VisitCallExpression(CallExpression call, CSharpEmitterContext context)
        {
            var compiled_args = call.Arguments
                                    .Select(arg => arg.AcceptWalker(this, context))
                                    .ToArray();
            var parent_args = context.ArgumentTypes;
            context.ArgumentTypes = call.OverloadSignature.Parameters.Select(p => CSharpCompilerHelpers.GetNativeType(p)).ToArray();// compiled_args.Select(arg => arg.Type).ToArray();

            context.RequestMethod = true;
            context.Method = null;
            var inst = call.Target.AcceptWalker(this, context);
            context.RequestMethod = false;
            context.ArgumentTypes = parent_args;
            return ConstructCallExpression(inst, context.Method, compiled_args);
        }

        public CSharpExpr VisitCastExpression(CastExpression castExpr, CSharpEmitterContext context)
        {
            var target = castExpr.Target.AcceptWalker(this, context);
            var to_type = CSharpCompilerHelpers.GetNativeType(castExpr.ToExpression);
            return CSharpExpr.Convert(target, to_type);
        }

        public CSharpExpr VisitCatchClause(CatchClause catchClause, CSharpEmitterContext context)
        {
            var ident = catchClause.Identifier;
            var exception_type = CSharpCompilerHelpers.GetNativeType((ident.Type.IdentifierNode.Type != null) ? ident.Type.IdentifierNode.Type : ident.Type);
            var param = CSharpExpr.Parameter(exception_type, ident.Name);
            AddSymbol(ident, new ExpressoSymbol{Parameter = param});

            var body = VisitBlock(catchClause.Body, context);
            context.CatchBlock = CSharpExpr.Catch(param, body);
            return null;
        }

        public CSharpExpr VisitClosureLiteralExpression(ClosureLiteralExpression closure, CSharpEmitterContext context)
        {
            var prev_additionals = context.Additionals;
            context.Additionals = new List<object>();

            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            var closure_type_builder = new LazyTypeBuilder(context.ModuleBuilder, "__Closure`" + ClosureId++, TypeAttributes.Class, Enumerable.Empty<Type>(), false);

            var formal_parameters = closure.Parameters.Select(p => p.AcceptWalker(this, context))
                                           .OfType<ExprTree.ParameterExpression>();
            var param_types = formal_parameters.Select(p => p.Type)
                                               .ToArray();

            var prev_context_closure = context.ContextClosureLiteral;
            context.ContextClosureLiteral = closure;

            var return_type = CSharpCompilerHelpers.GetNativeType(closure.ReturnType);
            var prev_return_target = ReturnTarget;
            var prev_default_return_value = DefaultReturnValue;
            ReturnTarget = CSharpExpr.Label(return_type, "ReturnTarget");
            DefaultReturnValue = CSharpExpr.Default(return_type);

            var closure_method_buider = closure_type_builder.DefineMethod(ClosureMethodName, MethodAttributes.Public, return_type, param_types);

            var field_idents = closure.LiftedIdentifiers
                                      .Select(ident => new {Name = ident.Name, Type = CSharpCompilerHelpers.GetNativeType(ident.Type)});
            foreach(var ctor_param in field_idents)
                closure_type_builder.DefineField(ctor_param.Name, ctor_param.Type, false);

            var param_ast_types = closure.Parameters.Select(p => p.ReturnType.Clone());
            var closure_func_type = AstType.MakeFunctionType("closure", closure.ReturnType.Clone(), param_ast_types);
            var closure_native_type = CSharpCompilerHelpers.GetNativeType(closure_func_type);
            var delegate_ctor = closure_native_type.GetConstructors().First();

            var closure_delegate_field = closure_type_builder.DefineField(ClosureDelegateName, closure_native_type, true);
            var interface_type = closure_type_builder.CreateInterfaceType();

            var prev_self = context.ParameterSelf;
            context.ParameterSelf = CSharpExpr.Parameter(interface_type, "self");

            var body = VisitBlock(closure.Body, context);
            var parameters = new []{context.ParameterSelf}.Concat(formal_parameters);
            var lambda = CSharpExpr.Lambda(body, parameters);
            closure_type_builder.SetBody(closure_method_buider, lambda);

            var lifted_params = closure.LiftedIdentifiers
                                       .Select(ident => VisitIdentifier(ident, context))
                                       .OfType<ExprTree.ParameterExpression>();

            closure_type_builder.SetBody(closure_delegate_field, (il, field) => {
                var closure_call_method = interface_type.GetMethod(ClosureMethodName);

                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldftn, closure_call_method);
                il.Emit(OpCodes.Newobj, delegate_ctor);
                il.Emit(OpCodes.Stfld, field);
            });
            var closure_type = closure_type_builder.CreateType();

            context.Additionals = prev_additionals;
            context.ContextClosureLiteral = prev_context_closure;
            context.ParameterSelf = prev_self;
            ReturnTarget = prev_return_target;
            DefaultReturnValue = prev_default_return_value;

            var ctor = closure_type.GetConstructors().First();
            var new_expr = CSharpExpr.New(ctor, lifted_params);

            var closure_call_target = closure_type.GetField(ClosureDelegateName);
            var member_access = CSharpExpr.Field(new_expr, closure_call_target);

            AscendScope();
            scope_counter = tmp_counter + 1;

            return member_access;
        }

        public CSharpExpr VisitComprehensionExpression(ComprehensionExpression comp, CSharpEmitterContext context)
        {
            var generator = comp.Item.AcceptWalker(this, context);
            context.ContextExpression = generator;
            var body = comp.Body.AcceptWalker(this, context);
            return body;
        }

        public CSharpExpr VisitComprehensionForClause(ComprehensionForClause compFor, CSharpEmitterContext context)
        {
            compFor.Left.AcceptWalker(this, context);
            compFor.Target.AcceptWalker(this, context);
            compFor.Body.AcceptWalker(this, context);
            return null;
        }

        public CSharpExpr VisitComprehensionIfClause(ComprehensionIfClause compIf, CSharpEmitterContext context)
        {
            if(compIf.Body.IsNull)      //[generator...if Condition] -> ...if(Condition) seq.Add(generator);
                return CSharpExpr.IfThen(compIf.Condition.AcceptWalker(this, context), context.ContextExpression);
            else                        //[...if Condition...] -> ...if(Condition){...}
                return CSharpExpr.IfThen(compIf.Condition.AcceptWalker(this, context), compIf.Body.AcceptWalker(this, context));
        }

        public CSharpExpr VisitConditionalExpression(ConditionalExpression condExpr, CSharpEmitterContext context)
        {
            var cond = condExpr.Condition.AcceptWalker(this, context);
            var true_result = condExpr.TrueExpression.AcceptWalker(this, context);
            var false_result = condExpr.FalseExpression.AcceptWalker(this, context);

            return CSharpExpr.Condition(cond, true_result, false_result);
        }

        public CSharpExpr VisitFinallyClause(FinallyClause finallyClause, CSharpEmitterContext context)
        {
            var body = VisitBlock(finallyClause.Body, context);
            return body;
        }

        public CSharpExpr VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue, CSharpEmitterContext context)
        {
            var ident = keyValue.KeyExpression as PathExpression;
            if(ident != null){
                if(context.TargetType == null)
                    throw new EmitterException("Can not create an object of UNKNOWN!");

                /*var field = context.TargetType.GetField(ident.AsIdentifier.Name);
                if(field == null){
                    throw new EmitterException(
                        "Type `{0}` does not have the field `{1}`.",
                        context.TargetType, ident.AsIdentifier.Name
                    );
                }
                context.Field = field;*/

                var value_expr = keyValue.ValueExpression.AcceptWalker(this, context);
                return value_expr;
            }else{
                // In a dictionary literal, the key can be any expression that is evaluated
                // to a hashable object.
                var key_expr = keyValue.KeyExpression.AcceptWalker(this, context);
                var value_expr = keyValue.ValueExpression.AcceptWalker(this, context);
                if(context.TargetType == null || !context.TargetType.Name.StartsWith("Dictionary", StringComparison.CurrentCulture))
                    context.TargetType = typeof(Dictionary<,>).MakeGenericType(key_expr.Type, value_expr.Type);

                var add_method = context.TargetType.GetMethod("Add");
                context.Additionals.Add(CSharpExpr.ElementInit(add_method, key_expr, value_expr));
                return null;
            }
        }

        public CSharpExpr VisitLiteralExpression(LiteralExpression literal, CSharpEmitterContext context)
        {
            if(literal.Type.Name == "bigint"){
                var ctor = typeof(BigInteger).GetMethod("Parse", new Type[]{
                    typeof(string)
                });
                return CSharpExpr.Call(ctor, CSharpExpr.Constant(literal.LiteralValue));
            }else{
                var native_type = CSharpCompilerHelpers.GetNativeType(literal.Type);
                return CSharpExpr.Constant(literal.Value, native_type);
            }
        }

        public CSharpExpr VisitIdentifier(Identifier ident, CSharpEmitterContext context)
        {
            if(context.ContextClosureLiteral != null){
                if(context.ContextClosureLiteral.LiftedIdentifiers.Any(i => i.Name == ident.Name))
                    return CSharpExpr.Field(context.ParameterSelf, ident.Name);
            }

            var symbol = GetRuntimeSymbol(ident);
            if(symbol != null){
                if(symbol.Parameter != null){
                    if(context.RequestType)
                        context.TargetType = symbol.Parameter.Type;

                    //CSharpExpr.DebugInfo(new ExprTree.SymbolDocumentInfo())
                    return symbol.Parameter;
                }else if(context.RequestPropertyOrField && symbol.PropertyOrField != null){
                    context.PropertyOrField = symbol.PropertyOrField;
                    return null;
                }else if(context.RequestType && symbol.Type != null){
                    context.TargetType = symbol.Type;
                    if(context.TargetType.GetConstructors().Any()){
                        // context.TargetType could be a static type
                        context.Constructor = context.TargetType.GetConstructors().Last();
                    }

                    return symbol.Parameter;
                }else if(context.RequestMethod){
                    if(symbol.Method == null)
                        throw new EmitterException("The native symbol '{0}' isn't defined", ident.Name);

                    context.Method = symbol.Method;
                    return null;
                }else{
                    throw new EmitterException("I can't guess what you want.");
                }
            }else{
                if(context.TargetType.IsEnum){
                    var enum_field = context.TargetType.GetField(ident.Name);
                    if(enum_field == null)
                        throw new EmitterException("It is found that the native symbol '{0}' doesn't represent an enum field.", ident.Name);

                    context.PropertyOrField = enum_field;
                    AddSymbol(ident, new ExpressoSymbol{PropertyOrField = enum_field});
                    return null;
                }else if(context.TargetType != null && context.RequestMethod){
                    // For methods or functions in external modules
                    // We regard types containing namespaces as types from other assemblies
                    //var method_name = context.TargetType.FullName.Contains(".") ? CSharpCompilerHelper.ConvertToPascalCase(ident.Name) : ident.Name;
                    var method = (context.ArgumentTypes != null) ? context.TargetType.GetMethod(ident.Name, context.ArgumentTypes) : context.TargetType.GetMethod(ident.Name);
                    if(method == null){
                        if(!context.RequestPropertyOrField)
                            throw new EmitterException("It is found that the native symbol '{0}' doesn't represent a method.", ident.Name);

                        var field = context.TargetType.GetField(ident.Name);
                        if(field == null){
                            var property = context.TargetType.GetProperty(ident.Name);
                            if(property == null)
                                throw new EmitterException("It is found that the native symbol '{0}' doesn't resolve to either a field, a property or a method.", ident.Name);

                            context.PropertyOrField = property;
                            AddSymbol(ident, new ExpressoSymbol{PropertyOrField = property});
                            return null;
                        }

                        context.PropertyOrField = field;
                        AddSymbol(ident, new ExpressoSymbol{PropertyOrField = field});
                    }
                    
                    context.Method = method;
                    // Don't add a method symbol so that we can find other overloads of it
                    //AddSymbol(ident, new ExpressoSymbol{Method = method});
                    return null;
                }else{
                    throw new EmitterException("It is found that the native symbol '{0}' isn't defined.", ident.Name);
                }
            }
        }

        public CSharpExpr VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq, CSharpEmitterContext context)
        {
            var intseq_ctor = typeof(ExpressoIntegerSequence).GetConstructor(new []{typeof(int), typeof(int), typeof(int), typeof(bool)});
            var args = new List<CSharpExpr>{
                intSeq.Start.AcceptWalker(this, context),
                intSeq.End.AcceptWalker(this, context),
                intSeq.Step.AcceptWalker(this, context),
                CSharpExpr.Constant(intSeq.UpperInclusive)
            };

            if(context.RequestMethod && context.Method == null)
                context.Method = typeof(ExpressoIntegerSequence).GetMethod("Includes");
            
            return CSharpExpr.New(intseq_ctor, args);      //new ExpressoIntegerSequence(Start, End, Step, UpperInclusive)
        }

        public CSharpExpr VisitIndexerExpression(IndexerExpression indexExpr, CSharpEmitterContext context)
        {
            var target = indexExpr.Target.AcceptWalker(this, context);
            var args = indexExpr.Arguments.Select(a => a.AcceptWalker(this, context));

            if(args.Count() == 1 && args.First().Type.Name == "ExpressoIntegerSequence"){
                var seq_type = target.Type;
                var elem_type = seq_type.IsArray ? seq_type.GetElementType() : seq_type.GenericTypeArguments[0];
                var ctor = typeof(Slice<,>).MakeGenericType(new []{seq_type, elem_type})
                                           .GetConstructor(new []{seq_type, typeof(ExpressoIntegerSequence)});
                return CSharpExpr.New(ctor, new []{target}.Concat(args));   // a[ExpressoIntegerSequence]
            }

            var type = target.Type;
            if(type.IsArray){
                return CSharpExpr.ArrayIndex(target, args);
            }else{
                var property_info = type.GetProperty("Item");
                return CSharpExpr.MakeIndex(target, property_info, args);
            }
        }

        public CSharpExpr VisitMemberReference(MemberReferenceExpression memRef, CSharpEmitterContext context)
        {
            // In Expresso, a member access can be resolved either to a field reference or an (instance and static) method call
            var expr = memRef.Target.AcceptWalker(this, context);
            context.RequestPropertyOrField = true;
            context.RequestMethod = true;
            context.PropertyOrField = null;
            context.Method = null;

            VisitIdentifier(memRef.Member, context);
            context.RequestPropertyOrField = false;
            context.RequestMethod = false;

            if(context.Method != null)
                return expr;
            else if(context.PropertyOrField is PropertyInfo)
                return CSharpExpr.Property(expr, (PropertyInfo)context.PropertyOrField);
            else
                return CSharpExpr.Field(expr, (FieldInfo)context.PropertyOrField);
        }

        public CSharpExpr VisitPathExpression(PathExpression pathExpr, CSharpEmitterContext context)
        {
            if(pathExpr.Items.Count == 1){
                context.RequestType = true;
                context.RequestMethod = true;
                context.TargetType = null;
                context.Method = null;

                var item = VisitIdentifier(pathExpr.AsIdentifier, context);
                context.RequestType = false;
                context.RequestMethod = false;
                return item;
            }

            context.TargetType = null;
            // We do this because the items in a path would already be resolved
            // and a path with more than 1 item can only refer to an external module's method
            var last_item = pathExpr.Items.Last();
            var native_symbol = GetRuntimeSymbol(last_item);
            if(native_symbol != null){
                context.TargetType = native_symbol.Type;
                context.Method = native_symbol.Method;
                context.PropertyOrField = native_symbol.PropertyOrField;
            }else{
                throw new EmitterException("It is found that the runtime symbol '{0}' doesn't represents anything", last_item.Name);
            }

            if(native_symbol.PropertyOrField != null)
                return CSharpExpr.Field(null, (FieldInfo)context.PropertyOrField);
            // On .NET environment, a path item is mapped to
            // Assembly::[Module]::{Class}
            // In reverse, an Expresso item can be mapped to the .NET type system as
            // Module.{Class}
            // Usually modules are converted to assemblies on themselves
            /*foreach(var ident in pathExpr.Items){
                if(context.TargetType == null){
                    var native_symbol = GetNativeSymbol(ident);
                    if(native_symbol != null)
                        context.TargetType = native_symbol.Type;
                    else
                        throw new EmitterException("Type `{0}` isn't defined!", ident.Name);
                }else if(context.TargetType != null){
                    // For methods or functions in external modules
                    var method_name = /*context.TargetType.FullName.StartsWith("System", StringComparison.CurrentCulture) ? CSharpCompilerHelper.ConvertToPascalCase(ident.Name);
                                             //: ident.Name;
                    var method = context.TargetType.GetMethod(method_name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    if(method == null)
                        throw new EmitterException("It is found that the native symbol '{0}' doesn't represent a method", ident.Name);

                    context.Method = method;
                    Symbols.Add(ident.IdentifierId, new ExpressoSymbol{Method = method});
                    return null;
                }else{
                    var type = context.TargetType.GetNestedType(ident.Name);
                    if(type != null){
                        context.TargetType = type;
                        if(context.Constructor == null)
                            context.Constructor = type.GetConstructors().Last();
                    }else{
                        throw new EmitterException("A nested type `{0}` isn't defined in Type `{1}`", ident.Name, context.TargetType.FullName);
                    }
                }
            }*/
            context.Assembly = null;

            return null;
        }

        public CSharpExpr VisitParenthesizedExpression(ParenthesizedExpression parensExpr, CSharpEmitterContext context)
        {
            return parensExpr.Expression.AcceptWalker(this, context);
        }

        public CSharpExpr VisitObjectCreationExpression(ObjectCreationExpression creation, CSharpEmitterContext context)
        {
            var args = new CSharpExpr[creation.Items.Count];
            context.Constructor = null;
            creation.TypePath.AcceptWalker(this, context);
            // Don't report TargetType missing error because it was already reported in TypeChecker
            //if(context.TargetType == null)
            //    throw new EmitterException("")
            var arg_types =
                from p in creation.CtorType.Parameters
                                  select CSharpCompilerHelpers.GetNativeType(p);
            context.Constructor = context.TargetType.GetConstructor(arg_types.ToArray());
            if(context.Constructor == null)
                throw new EmitterException("No constructors found for the path `{0}` with arguments {1}", creation, creation.TypePath, CSharpCompilerHelpers.ExpandContainer(arg_types));

            var formal_params = context.Constructor.GetParameters();
            // TODO: make object creation arguments pair to constructor parameters
            foreach(var pair in Enumerable.Range(0, creation.Items.Count()).Zip(
                creation.Items,
                (i, item) => Tuple.Create(i, item)
            )){
                context.PropertyOrField = null;
                var value_expr = pair.Item2.AcceptWalker(this, context);

                /*int index = 0;
                var key = context.Field.Name;
                if(!formal_params.Any(param => {
                    if(param.Name == key){
                        return true;
                    }else{
                        ++index;
                        return false;
                    }
                })){
                    throw new EmitterException(
                        "Can not create an instance with constructor `{0}` because it doesn't have field named `{1}`",
                        creation.TypePath, key
                    );
                }*/
                args[pair.Item1] = value_expr;
            }

            return CSharpExpr.New(context.Constructor, args);
        }

        public CSharpExpr VisitSequenceInitializer(SequenceInitializer seqInitializer, CSharpEmitterContext context)
        {
            var obj_type = seqInitializer.ObjectType;
            var seq_type = CSharpCompilerHelpers.GetContainerType(obj_type);
            // If this node represents a dictionary literal
            // context.Constructor will get set the appropriate constructor method.
            context.Constructor = null;
            var prev_additionals = context.Additionals;
            context.Additionals = new List<object>();
            // In order to force execution we call ToArray here because otherwise it results in a different code in Dictionary case
            var exprs = seqInitializer.Items.Select(item => item.AcceptWalker(this, context))
                                      .ToArray();

            if(seq_type == typeof(Array)){
                var elem_type = exprs.Any() ? exprs.First().Type : CSharpCompilerHelpers.GetNativeType(obj_type.TypeArguments.First());
                context.Additionals = prev_additionals;

                if(elem_type == typeof(ExpressoIntegerSequence)){
                    var array_create_method = typeof(ExpressoIntegerSequence).GetMethod("CreateArrayFromIntSeq");
                    return CSharpExpr.Call(array_create_method, exprs.First());
                }else{
                    return CSharpExpr.NewArrayInit(elem_type, exprs);
                }
            }else if(seq_type == typeof(List<>)){
                var elem_type = exprs.Any() ? exprs.First().Type : CSharpCompilerHelpers.GetNativeType(obj_type.TypeArguments.First());
                context.Additionals = prev_additionals;

                if(elem_type == typeof(ExpressoIntegerSequence)){
                    var list_create_method = typeof(ExpressoIntegerSequence).GetMethod("CreateListFromIntSeq");
                    return CSharpExpr.Call(list_create_method, exprs.First());
                }else{
                    var list_type = seq_type.MakeGenericType(elem_type);
                    var constructor = list_type.GetConstructor(new Type[]{});
                    var new_expr = CSharpExpr.New(constructor);
                    return !exprs.Any() ? new_expr : (CSharpExpr)CSharpExpr.ListInit(new_expr, exprs);
                }
            }else if(seq_type == typeof(Dictionary<,>)){
                var key_type = CSharpCompilerHelpers.GetNativeType(obj_type.TypeArguments.FirstOrNullObject());
                var value_type = CSharpCompilerHelpers.GetNativeType(obj_type.TypeArguments.LastOrNullObject());

                var elems = context.Additionals.Cast<ExprTree.ElementInit>();
                var dict_type = seq_type.MakeGenericType(key_type, value_type);
                var constructor = dict_type.GetConstructor(new Type[]{});
                var new_expr = CSharpExpr.New(constructor);
                context.Additionals = prev_additionals;
                return !exprs.Any() ? new_expr : (CSharpExpr)CSharpExpr.ListInit(new_expr, elems);
            }else if(seq_type == typeof(Tuple)){
                var child_types = 
                    from e in exprs
                    select e.Type;
                var ctor_method = typeof(Tuple).GetMethod("Create", child_types.ToArray());
                context.Additionals = prev_additionals;
                return CSharpExpr.Call(ctor_method, exprs);
            }else{
                throw new EmitterException("Could not emit code.");
            }
        }

        public CSharpExpr VisitMatchClause(MatchPatternClause matchClause, CSharpEmitterContext context)
        {
            // Since we can't translate the pattern matching directly to a C# expression
            // we have to translate it into equivalent expressions
            // e.g. match a {
            //          1 => print("1");
            //          2 | 3 => print("2 or 3");
            //          _ => print("otherwise");
            //      }
            // =>   if(a == 1){
            //          Console.Write("1");
            //      }else if(a == 2 || a == 3){
            //          Console.Write("2 or 3");
            //      }else{
            //          Console.Write("otherwise");
            //      }
            //
            // e.g.2 class Test
            //       {
            //           private let x (- int, y (- int;
            //       }
            //
            //       match t {
            //           Test{1, _} => print("1, x");,
            //           Test{2, 2} => print("2, 2");,
            //           Test{3, _} => print("3, x");,
            //           Test{x, y} if y == 2 * x => print("y == 2 * x");,
            //           Test{x, _} => print("{0}, y", x);,
            //           _          => ;
            //       }
            // =>    var __0 = t.x, __1 = t.y;
            //       if(__0 == 1){
            //           Console.Write("1, x");
            //       }else if(__0 == 2 && __1 == 2){
            //           Console.Write("2, 2");
            //       }else if(__0 == 3){
            //           Console.Write("3, x");
            //       }else{
            //           int x = __0, y = __1;    //destructuring becomes inner scope variable declarations
            //           if(y == 2 * x){          //a guard becomes an inner-scope if statement
            //               Console.Write("y == 2 * x");
            //           }
            //       }else{                       // a wildcard pattern becomes the else clause
            //       }
            // e.g.3 let x = [1, 2, 3];
            //       match x {
            //           [1, 2, x] => println("x is {0}", x);,
            //           [_, 2, x] => println("x is {0}", x);,
            //           [x, ..]   => println("x is {0}", x);
            //       }
            // =>    var __0 = x[0], __1 = x[1], __2 = x[2];  // in practice, the prefix is omitted
            //       if(__0 == 1 && __1 == 2){
            //           var x = __2;
            //           Console.WriteLine("x is {0}", x);
            //       }else if(__1 == 2){
            //           var x = __2;
            //           Console.WriteLine("x is {0}", x);
            //       }else if(x.Length > 1){
            //           var x = __0;
            //           Console.WriteLine("x is {0}", x);
            //       }
            // e.g.4 let t = (1, 'a', true);
            //       match t {
            //           (1, x, y) => println("x is {0} and y is {1}", x, y);,
            //           (1, 'a', _) => println("t is (1, 'a', _)");
            //       }
            // =>    var __0 = t.Item0, __1 = t.Item1, __2 = t.Item2; // in practice, the prefix is omitted
            //       if(__0 == 1){
            //           var x = __1, y = __2;
            //           Console.WriteLine("x is {0} and y is {1}", x, y);
            //       }else if(__0 == 1 && __1 == 'a'){
            //           Console.WriteLine("t is (1, 'a', _)");
            //       }
            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            // Define identifiers before inspecting the body statement.
            matchClause.AcceptWalker(identifier_definer);

            var body = matchClause.Body.AcceptWalker(this, context);
            context.ContextExpression = body;

            CSharpExpr res = null;
            foreach(var pattern in matchClause.Patterns){
                var pattern_cond = pattern.AcceptWalker(this, context);
                if(res == null)
                    res = pattern_cond;
                else
                    res = CSharpExpr.OrElse(res, pattern_cond);
            }

            /*if(destructuring_exprs.Count() != context.Additionals.Count()){
                // The number of destructured variables must match in every pattern
                throw new EmitterException(
                    "Expected the pattern contains {0} variables, but it only contains {1}!",
                    destructuring_exprs.Count(), context.Additionals.Count()
                );
            }*/

            var guard = matchClause.Guard.AcceptWalker(this, context);
            if(guard != null)
                res = CSharpExpr.AndAlso(res, guard);

            AscendScope();
            scope_counter = tmp_counter + 1;

            return (res == null) ? null : CSharpExpr.IfThen(res, context.ContextExpression);
        }

        public CSharpExpr VisitSequenceExpression(SequenceExpression seqExpr, CSharpEmitterContext context)
        {
            // A sequence expression is always translated to a tuple
            var exprs = new List<CSharpExpr>();
            var types = new List<Type>();
            foreach(var item in seqExpr.Items){
                var tmp = item.AcceptWalker(this, context);
                types.Add(tmp.Type);
                exprs.Add(tmp);
            }

            if(types.Count == 1){
                return exprs.First();
            }else{
                var ctor_method = typeof(Tuple).GetGenericMethod("Create", BindingFlags.Public | BindingFlags.Static, types.ToArray());

                return CSharpExpr.Call(ctor_method, exprs);
            }
        }

        public CSharpExpr VisitUnaryExpression(UnaryExpression unaryExpr, CSharpEmitterContext context)
        {
            var operand = unaryExpr.Operand.AcceptWalker(this, context);
            return ConstructUnaryOp(operand, unaryExpr.Operator);
        }

        public CSharpExpr VisitSelfReferenceExpression(SelfReferenceExpression selfRef, CSharpEmitterContext context)
        {
            return context.ParameterSelf;
        }

        public CSharpExpr VisitSuperReferenceExpression(SuperReferenceExpression superRef, CSharpEmitterContext context)
        {
            var super_type = context.LazyTypeBuilder.BaseType;
            return CSharpExpr.Parameter(super_type, "super");
        }

        public CSharpExpr VisitNullReferenceExpression(NullReferenceExpression nullRef, CSharpEmitterContext context)
        {
            return CSharpExpr.Constant(null, typeof(object));
        }

        public CSharpExpr VisitCommentNode(CommentNode comment, CSharpEmitterContext context)
        {
            // Just ignore comment nodes...
            return null;
        }

        public CSharpExpr VisitTextNode(TextNode textNode, CSharpEmitterContext context)
        {
            // Just ignore text nodes, too...
            return null;
        }

        // AstType nodes should be treated with special care
        public CSharpExpr VisitSimpleType(SimpleType simpleType, CSharpEmitterContext context)
        {
            var symbol = GetRuntimeSymbol(simpleType.IdentifierNode);
            if(symbol != null && symbol.Type != null){
                context.TargetType = symbol.Type;
                return symbol.Parameter;
            }else{
                throw new EmitterException("It is found that Type '{0}' isn't defined.", simpleType.Identifier);
            }
        }

        public CSharpExpr VisitPrimitiveType(PrimitiveType primitiveType, CSharpEmitterContext context)
        {
            return null;
        }

        public CSharpExpr VisitReferenceType(ReferenceType referenceType, CSharpEmitterContext context)
        {
            return null;
        }

        public CSharpExpr VisitMemberType(MemberType memberType, CSharpEmitterContext context)
        {
            // We don't need to inspect memberType.Target because memberType.IdentifierNode is already binded
            //memberType.Target.AcceptWalker(this, context);
            context.RequestType = true;
            VisitSimpleType(memberType.ChildType, context);
            context.RequestType = false;
            return null;
        }

        public CSharpExpr VisitFunctionType(FunctionType funcType, CSharpEmitterContext context)
        {
            return null;
        }

        public CSharpExpr VisitParameterType(ParameterType paramType, CSharpEmitterContext context)
        {
            return null;
        }

        public CSharpExpr VisitPlaceholderType(PlaceholderType placeholderType, CSharpEmitterContext context)
        {
            return null;
        }

        public CSharpExpr VisitAliasDeclaration(AliasDeclaration aliasDecl, CSharpEmitterContext context)
        {
            context.TargetType = null;
            context.Method = null;
            aliasDecl.Path.AcceptWalker(this, context);
            if(context.Method == null && context.TargetType == null)
                throw new EmitterException("`{0}` could not be resolved to an entity name!", aliasDecl.Path);

            AddSymbol(aliasDecl.AliasToken, new ExpressoSymbol{
                Type = context.TargetType,
                Method = context.Method
            });
            return null;
        }

        public CSharpExpr VisitImportDeclaration(ImportDeclaration importDecl, CSharpEmitterContext context)
        {
            foreach(var pair in importDecl.ImportPaths.Zip(importDecl.AliasTokens, (l, r) => new Tuple<Identifier, Identifier>(l, r))){
                if(!pair.Item1.Name.Contains("::") && !pair.Item1.Name.Contains("."))
                    break;
                
                var import_path = pair.Item1;
                var alias = pair.Item2;

                var last_index = import_path.Name.LastIndexOf("::", StringComparison.CurrentCulture);
                var type_name = import_path.Name.Substring(last_index == -1 ? 0 : last_index + 2);
                var type = ('a' <= type_name[0] && type_name[0] <= 'z') ? null : CSharpCompilerHelpers.GetNativeType(AstType.MakeSimpleType(CSharpCompilerHelpers.ConvertToPascalCase(type_name)));

                if(type != null){
                    var expresso_symbol = new ExpressoSymbol{Type = type};
                    //AddSymbol(import_path, expresso_symbol);
                    // This if statement is needed because otherwise types in the standard library
                    // won't get cached
                    if(!Symbols.ContainsKey(alias.IdentifierId))
                        AddSymbol(alias, expresso_symbol);
                }else{
                    var symbol = GetRuntimeSymbol(alias);
                    if(symbol != null){
                        //AddSymbol(alias, symbol);
                    }else{
                        var module_type_name = import_path.Name.Substring(0, last_index);
                        var module_type = CSharpCompilerHelpers.GetNativeType(AstType.MakeSimpleType(module_type_name));
                        var member_name = type_name;//CSharpCompilerHelper.ConvertToPascalCase(type_name);

                        if(module_type != null){
                            var flags = BindingFlags.Static | BindingFlags.NonPublic;
                            var method = module_type.GetMethod(member_name, flags);

                            if(method != null){
                                AddSymbol(alias, new ExpressoSymbol{Method = method});
                            }else{
                                var module_field = module_type.GetField(member_name, flags);

                                if(module_field != null)
                                    AddSymbol(alias, new ExpressoSymbol{PropertyOrField = module_field});
                                else
                                    throw new EmitterException("Error ES1903: It is found that the import name {0} isn't defined", import_path.Name);
                            }
                        }
                    }
                }
            }

            return null;
        }

        public CSharpExpr VisitFunctionDeclaration(FunctionDeclaration funcDecl, CSharpEmitterContext context)
        {
            if(funcDecl.Body.IsNull)
                return null;
            
            context.Additionals = new List<object>();
            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            var context_ast = context.ContextAst;
            context.ContextAst = funcDecl;

            var formal_parameters = funcDecl.Parameters.Select(param => param.AcceptWalker(this, context))
                                            .OfType<ExprTree.ParameterExpression>();

            var return_type = CSharpCompilerHelpers.GetNativeType(funcDecl.ReturnType);
            ReturnTarget = CSharpExpr.Label(return_type, "returnTarget");
            DefaultReturnValue = CSharpExpr.Default(return_type);

            var prev_self = context.ParameterSelf;
            var self_type = context.LazyTypeBuilder.InterfaceType;
            context.ParameterSelf = CSharpExpr.Parameter(self_type, "self");

            var is_global_function = !funcDecl.Modifiers.HasFlag(Modifiers.Public) && !funcDecl.Modifiers.HasFlag(Modifiers.Protected) && !funcDecl.Modifiers.HasFlag(Modifiers.Private);
            var parameters = is_global_function ? formal_parameters : new []{context.ParameterSelf}.Concat(formal_parameters);

            var attrs = is_global_function ? BindingFlags.Static : BindingFlags.Instance;
            if(funcDecl.Modifiers.HasFlag(Modifiers.Export) || funcDecl.Modifiers.HasFlag(Modifiers.Public))
                attrs |= BindingFlags.Public;
            else
                attrs |= BindingFlags.NonPublic;

            var interface_func = context.LazyTypeBuilder.GetInterfaceMethod(funcDecl.Name, attrs);//context.LazyTypeBuilder.GetInterfaceMethod(CSharpCompilerHelper.ConvertToPascalCase(funcDecl.Name), attrs);
            AddSymbol(funcDecl.NameToken, new ExpressoSymbol{Method = interface_func});

            var body = funcDecl.Body.AcceptWalker(this, context);
            context.Additionals = null;
            context.ContextAst = context_ast;
            context.ParameterSelf = prev_self;
            var lambda = CSharpExpr.Lambda(body, parameters);

            if(funcDecl.Name == "main")
                context.AssemblyBuilder.SetEntryPoint(interface_func, PEFileKinds.ConsoleApplication);

            context.LazyTypeBuilder.SetBody(interface_func, lambda);

            /*if(funcDecl.Parent is TypeDeclaration type_decl){
                foreach(var base_type in type_decl.BaseTypes){
                    var interface_type = Symbols[base_type.IdentifierNode.IdentifierId].Type;
                    var interface_method = interface_type.GetMethod(funcDecl.Name);
                    if(interface_method != null)
                        context.TypeBuilder.InterfaceTypeBuilder.DefineMethodOverride(interface_func, interface_method);
                }
            }*/

            AscendScope();
            scope_counter = tmp_counter + 1;

            return null;
        }

        public CSharpExpr VisitTypeDeclaration(TypeDeclaration typeDecl, CSharpEmitterContext context)
        {
            var original_count = scope_counter;
            var interface_definer = new InterfaceTypeDefiner(this, context);
            interface_definer.VisitTypeDeclaration(typeDecl);

            scope_counter = original_count;

            var parent_type = context.LazyTypeBuilder;
            context.LazyTypeBuilder = Symbols[typeDecl.NameToken.IdentifierId].TypeBuilder;

            DescendScope();
            scope_counter = 0;

            try{
                foreach(var member in typeDecl.Members)
                    member.AcceptWalker(this, context);

                if(typeDecl.TypeKind == ClassType.Class)
                    context.LazyTypeBuilder.CreateType();
            }
            finally{
                context.LazyTypeBuilder = parent_type;
            }

            AscendScope();
            scope_counter = original_count + 1;
            return null;
        }

        public CSharpExpr VisitFieldDeclaration(FieldDeclaration fieldDecl, CSharpEmitterContext context)
        {
            foreach(var init in fieldDecl.Initializers){
                var field_builder = init.NameToken != null ? Symbols[init.NameToken.IdentifierId].PropertyOrField as FieldBuilder : throw new EmitterException("Invalid field: {0}", init.Pattern);
                var initializer = init.Initializer.AcceptWalker(this, context);
                if(initializer != null)
                    context.LazyTypeBuilder.SetBody(field_builder, initializer);
            }

            return null;
        }

        public CSharpExpr VisitParameterDeclaration(ParameterDeclaration parameterDecl, CSharpEmitterContext context)
        {
            ExprTree.ParameterExpression param;
            if(!Symbols.ContainsKey(parameterDecl.NameToken.IdentifierId)){
                var native_type = CSharpCompilerHelpers.GetNativeType(parameterDecl.ReturnType);
                param = CSharpExpr.Parameter(native_type, parameterDecl.Name);
                AddSymbol(parameterDecl.NameToken, new ExpressoSymbol{Parameter = param});
            }else{
                param = (ExprTree.ParameterExpression)VisitIdentifier(parameterDecl.NameToken, context);
            }
            if(context.Additionals != null)
                context.Additionals.Add(param);

            return param;
        }

        public CSharpExpr VisitVariableInitializer(VariableInitializer initializer, CSharpEmitterContext context)
        {
            var prev_params = context.AdditionalParameters;
            var prev_addtionals = context.Additionals;
            context.AdditionalParameters = new List<ExprTree.ParameterExpression>();
            context.Additionals = new List<object>();
            initializer.Pattern.AcceptWalker(this, context);
            var variables = context.AdditionalParameters;
            var debug_infos = context.Additionals.OfType<CSharpExpr>();
            context.AdditionalParameters = prev_params;
            context.Additionals = prev_addtionals;

            var init = initializer.Initializer.AcceptWalker(this, context);

            CSharpExpr result;
            if(init == null){
                result = CSharpExpr.Block(variables, options.BuildType.HasFlag(BuildType.Debug) ? debug_infos : new []{CSharpExpr.Empty()});
            }else{
                if(variables.Count > 1){
                    if(init is ExprTree.MethodCallExpression call_expr){
                        // let (t1, t2) = Tuple.Create(...);
                        var assignments = variables.Zip(call_expr.Arguments, (l, r) => new Tuple<ExprTree.ParameterExpression, CSharpExpr>(l, r))
                                                   .Select(pair => CSharpExpr.Assign(pair.Item1, pair.Item2));
                        var debug_info_list = debug_infos.ToList();
                        debug_info_list.AddRange(assignments);
                        result = options.BuildType.HasFlag(BuildType.Debug) ? CSharpExpr.Block(debug_info_list) : CSharpExpr.Block(assignments);
                    }else{
                        // let (t1, t2) = t where t is Tuple
                        var tmps = new List<CSharpExpr>();
                        int i = 1;
                        foreach(var variable in variables){
                            tmps.Add(CSharpExpr.Assign(variable, CSharpExpr.Property(init, "Item" + i)));
                            ++i;
                        }

                        var debug_info_list = debug_infos.ToList();
                        debug_info_list.AddRange(tmps);
                        result = CSharpExpr.Block(debug_info_list);//options.BuildType.HasFlag(BuildType.Debug) ? CSharpExpr.Block(debug_info_list) : CSharpExpr.Block(tmps);
                    }
                }else{
                    var assignment = CSharpExpr.Assign(variables[0], init);
                    if(options.BuildType.HasFlag(BuildType.Debug)){
                        var debug_info_list = debug_infos.ToList();
                        debug_info_list.Add(assignment);
                        result = CSharpExpr.Block(debug_info_list);
                    }else{
                        result = assignment;
                    }
                }
            }

            if(context.Additionals != null)
                context.Additionals.Add(result);
            
            return result;
        }

        //#################################################
        //# Patterns can either be lvalues or rvalues
        //# Guideline for pattern visit methods:
        //# All pattern visit methods should meet the following preconditions and requirements
        //# Preconditions:
        //#     context.ContextExpression refers to the current state of the body block
        //# Requirements:
        //#     context.ContextExpression represents the expressions the corresponding body block should care for
        //#     and the return value indicates the branch condition(in other words, it indicates the condition
        //#     that the pattern should branch on)
        //#################################################
        public CSharpExpr VisitWildcardPattern(WildcardPattern wildcardPattern, CSharpEmitterContext context)
        {
            // A wildcard pattern is translated to the else clause
            // so just return null to indicate that.
            return null;
        }

        public CSharpExpr VisitIdentifierPattern(IdentifierPattern identifierPattern, CSharpEmitterContext context)
        {
            // An identifier pattern can arise by itself or as a child
            ExprTree.ParameterExpression param = null;
            if(!(context.ContextAst is MatchStatement)){
                param = CSharpExpr.Parameter(CSharpCompilerHelpers.GetNativeType(identifierPattern.Identifier.Type), identifierPattern.Identifier.Name);
                AddSymbol(identifierPattern.Identifier, new ExpressoSymbol{Parameter = param});

                var start_loc = identifierPattern.Identifier.StartLocation;
                var end_loc = identifierPattern.Identifier.EndLocation;
                var debug_info = CSharpExpr.DebugInfo(document_info, start_loc.Line, start_loc.Column, end_loc.Line, end_loc.Column);
                if(context.AdditionalParameters != null)
                    context.AdditionalParameters.Add(param);

                if(context.Additionals != null)
                    context.Additionals.Add(debug_info);
            }else{
                param = (ExprTree.ParameterExpression)identifierPattern.Identifier.AcceptWalker(this, context);
            }

            if(identifierPattern.Parent is MatchPatternClause){
                // Set context.ContextExpression to a block
                //var native_type = CSharpCompilerHelper.GetNativeType(identifierPattern.Identifier.Type);
                param = GetRuntimeSymbol(identifierPattern.Identifier).Parameter;
                var assignment = CSharpExpr.Assign(param, context.TemporaryVariable);
                context.ContextExpression = CSharpExpr.Block(new []{param}, new []{assignment, context.ContextExpression});
            }

            if(context.PropertyOrField == null && context.TargetType != null && context.ContextAst is MatchStatement 
               && (identifierPattern.Parent is DestructuringPattern || identifierPattern.Parent is TuplePattern)){
                // context.TargetType is supposed to be set in CSharpEmitter.VisitIdentifier
                var field = context.TargetType.GetField(identifierPattern.Identifier.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if(field == null){
                    throw new EmitterException(
                        "The type `{0}` doesn't have the field `{1}`",
                        context.TargetType, identifierPattern.Identifier.Name
                    );
                }
                context.PropertyOrField = field;
            }

            if(!identifierPattern.InnerPattern.IsNull)
                return identifierPattern.InnerPattern.AcceptWalker(this, context);
            else
                return param;
        }

        public CSharpExpr VisitCollectionPattern(CollectionPattern collectionPattern, CSharpEmitterContext context)
        {
            // First, make type validation expression
            var collection_type = CSharpCompilerHelpers.GetContainerType(collectionPattern.CollectionType);
            var item_type = CSharpCompilerHelpers.GetNativeType(collectionPattern.CollectionType.TypeArguments.First());
            var prev_additionals = context.Additionals;
            context.Additionals = new List<object>();
            var prev_additional_params = context.AdditionalParameters;
            context.AdditionalParameters = new List<ExprTree.ParameterExpression>();

            CSharpExpr res = null;
            int i = 0;
            var block = new List<CSharpExpr>();
            var block_params = new List<ExprTree.ParameterExpression>();
            foreach(var pattern in collectionPattern.Items){
                var index = CSharpExpr.Constant(i++);
                var elem_access = CSharpExpr.ArrayIndex(context.TemporaryExpression, index);
                //var tmp_param = CSharpExpr.Parameter(item_type, "__" + VariableCount++);
                //var assignment = CSharpExpr.Assign(tmp_param, elem_access);
                //context.Additionals.Add(assignment);
                //context.AdditionalParameters.Add(tmp_param);
                //block.Add(assignment);
                //block_params.Add(tmp_param);

                var prev_tmp_expr = context.TemporaryExpression;
                context.TemporaryExpression = elem_access;
                var expr = pattern.AcceptWalker(this, context);
                context.TemporaryExpression = prev_tmp_expr;

                var param = expr as ExprTree.ParameterExpression;
                if(param != null){
                    var assignment2 = CSharpExpr.Assign(param, elem_access);
                    block.Add(assignment2);
                    block_params.Add(param);
                }else{
                    if(context.Additionals.Any()){
                        var block_contents = context.Additionals.OfType<ExprTree.Expression>().ToList();
                        if(expr != null){
                            var if_content = CSharpExpr.IfThen(expr, context.ContextExpression);
                            block_contents.Add(if_content);
                        }
                        context.ContextExpression = CSharpExpr.Block(context.AdditionalParameters, block_contents);
                    }if(res == null){
	                    res = expr;
                    }else{
	                    res = CSharpExpr.AndAlso(res, expr);
                    }
                }
            }

            if(res == null){
                var native_type = CSharpCompilerHelpers.GetNativeType(collectionPattern.CollectionType);
                res = CSharpExpr.TypeIs(context.TemporaryVariable, native_type);
            }

            if(res != null)
                block.Add(CSharpExpr.IfThen(res, context.ContextExpression));
            else
                block.Add(context.ContextExpression);

            context.Additionals = prev_additionals;
            context.AdditionalParameters = prev_additional_params;

            if(block.Any())
                context.ContextExpression = CSharpExpr.Block(block_params, block);

            return res;//CSharpExpr.TypeIs(context.TemporaryVariable, collection_type);
        }

        public CSharpExpr VisitDestructuringPattern(DestructuringPattern destructuringPattern, CSharpEmitterContext context)
        {
            context.TargetType = null;
            destructuringPattern.TypePath.AcceptWalker(this, context);

            var type = context.TargetType;
            context.RequestPropertyOrField = true;
            var prev_additionals = context.Additionals;
            context.Additionals = new List<object>();
            var prev_additional_params = context.AdditionalParameters;
            context.AdditionalParameters = new List<ExprTree.ParameterExpression>();

            CSharpExpr res = null;
            var block = new List<CSharpExpr>();
            var block_params = new List<ExprTree.ParameterExpression>();
            foreach(var pattern in destructuringPattern.Items){
                var item_ast_type = pattern.AcceptWalker(item_type_inferencer);
                if(item_ast_type == null)
                    continue;
                
                var item_type = CSharpCompilerHelpers.GetNativeType(item_ast_type);
                //var tmp_param = CSharpExpr.Parameter(item_type, "__" + VariableCount++);

                //var prev_tmp_variable = context.TemporaryVariable;
                //context.TemporaryVariable = tmp_param;
                context.PropertyOrField = null;
                var expr = pattern.AcceptWalker(this, context);
                //context.TemporaryVariable = prev_tmp_variable;

                var field_access = CSharpExpr.Field(context.TemporaryExpression, (FieldInfo)context.PropertyOrField);
                //var assignment = CSharpExpr.Assign(tmp_param, field_access);
                //context.Additionals.Add(assignment);
                //context.AdditionalParameters.Add(tmp_param);
                //block.Add(assignment);
                //block_params.Add(tmp_param);

                var param = expr as ExprTree.ParameterExpression;
                if(param != null){
                    var assignemnt2 = CSharpExpr.Assign(param, field_access);
                    block_params.Add(param);
                    block.Add(assignemnt2);
                }else{
                    if(context.Additionals.Any()){
                        var block_contents = context.Additionals.OfType<ExprTree.Expression>().ToList();
                        if(expr != null){
                            var if_content = CSharpExpr.IfThen(expr, context.ContextExpression);
                            block_contents.Add(if_content);
                        }
                        context.ContextExpression = CSharpExpr.Block(context.AdditionalParameters, block_contents);
                    }else if(res == null){
                        res = expr;
                    }else{
                        res = CSharpExpr.AndAlso(res, expr);
                    }
                }
            }

            if(res == null){
                var native_type = CSharpCompilerHelpers.GetNativeType(destructuringPattern.TypePath);
                res = CSharpExpr.TypeIs(context.TemporaryVariable, native_type);
            }

            if(res != null)
                block.Add(CSharpExpr.IfThen(res, context.ContextExpression));
            else
                block.Add(context.ContextExpression);

            context.Additionals = prev_additionals;
            context.AdditionalParameters = prev_additional_params;
            
            context.RequestPropertyOrField = false;
            if(block.Any())
                context.ContextExpression = CSharpExpr.Block(block_params, block);

            return res;//CSharpExpr.TypeIs(context.TemporaryVariable, type);
        }

        public CSharpExpr VisitTuplePattern(TuplePattern tuplePattern, CSharpEmitterContext context)
        {
            // Tuple patterns should always be combined with value binding patterns
            if(tuplePattern.Ancestors.Any(a => a is MatchStatement)){
                var prev_additionals = context.Additionals;
                context.Additionals = new List<object>();
                var prev_addtional_params = context.AdditionalParameters;
                context.AdditionalParameters = new List<ExprTree.ParameterExpression>();

                var element_types = new List<Type>();
                var block_params = new List<ExprTree.ParameterExpression>();
                var block = new List<CSharpExpr>();
                CSharpExpr res = null;
                int i = 1;
                foreach(var pattern in tuplePattern.Patterns){
                    var item_ast_type = pattern.AcceptWalker(item_type_inferencer);
                    if(item_ast_type == null)
                        continue;
                    
                    var item_type = CSharpCompilerHelpers.GetNativeType(item_ast_type);
                    //var tmp_param = CSharpExpr.Parameter(item_type, "__" + VariableCount++);
                    var prop_name = "Item" + i++;
                    var property_access = CSharpExpr.Property(context.TemporaryExpression, prop_name);
                    //var assignment = CSharpExpr.Assign(tmp_param, property_access);
                    element_types.Add(item_type);
                    //context.Additionals.Add(assignment);
                    //context.AdditionalParameters.Add(tmp_param);
                    //block.Add(assignment);
                    //block_params.Add(tmp_param);

                    var prev_tmp_expr = context.TemporaryExpression;
                    context.TemporaryExpression = property_access;
                    var expr = pattern.AcceptWalker(this, context);
                    context.TemporaryExpression = prev_tmp_expr;

                    var param = expr as ExprTree.ParameterExpression;
                    if(param != null){
                        var assignment2 = CSharpExpr.Assign(param, property_access);
                        block.Add(assignment2);
                        block_params.Add(param);
                    }else{
                        if(context.Additionals.Any()){
                            var block_contents = context.Additionals.OfType<ExprTree.Expression>().ToList();
                            if(expr != null){
                                var if_content = CSharpExpr.IfThen(expr, context.ContextExpression);
                                block_contents.Add(if_content);
                            }
                            context.ContextExpression = CSharpExpr.Block(context.AdditionalParameters, block_contents);
                        }else if(res == null){
                            res = expr;
                        }else{
                            res = CSharpExpr.AndAlso(res, expr);
                        }
                    }
                }

                if(res == null){
                    var tuple_type = CSharpCompilerHelpers.GuessTupleType(element_types);
                    res = CSharpExpr.TypeIs(context.TemporaryVariable, tuple_type);
                }

                if(res != null)
                    block.Add(CSharpExpr.IfThen(res, context.ContextExpression));
                else
                    block.Add(context.ContextExpression);

                context.Additionals = prev_additionals;
                context.AdditionalParameters = prev_addtional_params;
                
                if(block.Any())
                    context.ContextExpression = CSharpExpr.Block(block_params, block);

                return res;//CSharpExpr.TypeIs(context.TemporaryVariable, tuple_type);
            }else{
                foreach(var pattern in tuplePattern.Patterns)
                    pattern.AcceptWalker(this, context);

                return null;
            }
        }

        public CSharpExpr VisitExpressionPattern(ExpressionPattern exprPattern, CSharpEmitterContext context)
        {
            // Common scinario in an expression pattern:
            // An integer sequence expression or a literal expression.
            // In the former case we should test an integer against an IntSeq type object using an IntSeq's method
            // while in the latter case we should just test the value against the literal
            context.RequestMethod = true;
            context.Method = null;
            var expr = exprPattern.Expression.AcceptWalker(this, context);
            context.RequestMethod = false;
            return (context.Method != null && context.Method.DeclaringType.Name == "ExpressoIntegerSequence") ? CSharpExpr.Call(expr, context.Method, context.TemporaryVariable) :
                                                                                                                          (context.ContextAst is MatchStatement) ? CSharpExpr.Equal(context.TemporaryVariable, expr)  : expr;
        }

        public CSharpExpr VisitIgnoringRestPattern(IgnoringRestPattern restPattern, CSharpEmitterContext context)
        {
            return null;
        }

        public CSharpExpr VisitKeyValuePattern(KeyValuePattern keyValuePattern, CSharpEmitterContext context)
        {
            context.PropertyOrField = null;
            context.RequestPropertyOrField = true;
            VisitIdentifier(keyValuePattern.KeyIdentifier, context);
            var expr = keyValuePattern.Value.AcceptWalker(this, context);

            var param = expr as ExprTree.ParameterExpression;
            if(param != null){
                var assignment = CSharpExpr.Assign(param, CSharpExpr.Field(context.TemporaryVariable, (FieldInfo)context.PropertyOrField));
                return assignment;
            }
            return expr;
        }

        public CSharpExpr VisitPatternWithType(PatternWithType pattern, CSharpEmitterContext context)
        {
            return pattern.Pattern.AcceptWalker(this, context);
        }

        public CSharpExpr VisitNullNode(AstNode nullNode, CSharpEmitterContext context)
        {
            // Just ignore null nodes...
            return null;
        }

        public CSharpExpr VisitNewLine(NewLineNode newlineNode, CSharpEmitterContext context)
        {
            // Just ignore new lines...
            return null;
        }

        public CSharpExpr VisitWhitespace(WhitespaceNode whitespaceNode, CSharpEmitterContext context)
        {
            // Just ignore whitespaces...
            return null;
        }

        public CSharpExpr VisitExpressoTokenNode(ExpressoTokenNode tokenNode, CSharpEmitterContext context)
        {
            // It doesn't matter what tokens Expresso uses
            return null;
        }

        public CSharpExpr VisitPatternPlaceholder(AstNode placeholder, ICSharpCode.NRefactory.PatternMatching.Pattern child, CSharpEmitterContext context)
        {
            // Ignore placeholder nodes because they are just placeholders...
            return null;
        }

        #endregion

		#region methods
		CSharpExpr ConstructBinaryOp(CSharpExpr lhs, CSharpExpr rhs, OperatorType opType)
		{
			switch(opType){
			case OperatorType.ConditionalAnd:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.AndAlso, lhs, rhs);

			case OperatorType.BitwiseAnd:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.And, lhs, rhs);

			case OperatorType.BitwiseShiftLeft:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.LeftShift, lhs, rhs);

			case OperatorType.BitwiseOr:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.Or, lhs, rhs);

			case OperatorType.BitwiseShiftRight:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.RightShift, lhs, rhs);

			case OperatorType.ExclusiveOr:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.ExclusiveOr, lhs, rhs);

			case OperatorType.Divide:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.Divide, lhs, rhs);

			case OperatorType.Equality:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.Equal, lhs, rhs);

			case OperatorType.GreaterThan:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.GreaterThan, lhs, rhs);

			case OperatorType.GreaterThanOrEqual:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.GreaterThanOrEqual, lhs, rhs);

			case OperatorType.LessThanOrEqual:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.LessThanOrEqual, lhs, rhs);

			case OperatorType.LessThan:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.LessThan, lhs, rhs);

			case OperatorType.Minus:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.Subtract, lhs, rhs);

			case OperatorType.Modulus:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.Modulo, lhs, rhs);

			case OperatorType.InEquality:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.NotEqual, lhs, rhs);

			case OperatorType.ConditionalOr:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.OrElse, lhs, rhs);

			case OperatorType.Plus:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.Add, lhs, rhs);

			case OperatorType.Power:
                var prev_type = lhs.Type;
                lhs = CSharpExpr.Convert(lhs, typeof(double));
                rhs = CSharpExpr.Convert(rhs, typeof(double));
                return CSharpExpr.Convert(CSharpExpr.MakeBinary(ExprTree.ExpressionType.Power, lhs, rhs), prev_type);

			case OperatorType.Times:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.Multiply, lhs, rhs);

			default:
				throw new EmitterException("Unknown binary operator!");
			}
		}

        CSharpExpr ConstructUnaryOp(CSharpExpr operand, OperatorType opType)
		{
			switch(opType){
            case OperatorType.Reference:
                // The parameter modifier "ref" is the primary candidate for this operator and it is expressed as a type
                // So we don't need to do anything here :-)
                return operand;

			case OperatorType.Plus:
				return CSharpExpr.UnaryPlus(operand);

			case OperatorType.Minus:
				return CSharpExpr.Negate(operand);

			case OperatorType.Not:
				return CSharpExpr.Not(operand);

			default:
				throw new EmitterException("Unknown unary operator!");
			}
		}

        CSharpExpr ConstructAssignment(CSharpExpr lhs, CSharpExpr rhs)
        {
            // C# doesn't have alternative expressions to patterns
            // so translate them to semantically equivalent expressions
            // taking transformation into account.

            // See if the left-hand-side represents destructuring or not
            return null;
        }

        IEnumerable<ExprTree.ParameterExpression> ConvertSymbolsToParameters()
        {
            var parameters =
                from symbol in symbol_table.Symbols
                where symbol.IdentifierId != 0
                group symbol by symbol.IdentifierId into same_values
                from unique_symbol in same_values
                let param = Symbols[unique_symbol.IdentifierId]
                select param.Parameter;

            return parameters;
        }

        CSharpExpr ConstructCallExpression(CSharpExpr inst, MethodInfo method, CSharpExpr[] args)
        {
            if(method == null){
                return CSharpExpr.Invoke(inst, args);
            }else if(method.DeclaringType.Name == "Console" && (method.Name == "Write" || method.Name == "WriteLine") ||
                     method.DeclaringType.Name == "String" && method.Name == "Format"){
                var first = args.First();
                var expand_method = typeof(CSharpCompilerHelpers).GetMethod("ExpandContainer");
                var first_string = first as ExprTree.ConstantExpression;
                if(first.Type == typeof(string) && first_string != null && ((string)first_string.Value).Contains("{0}")){
                    // Only string.Format takes this path
                    var parameters = method.GetParameters();
                    switch(parameters.Length){
                    case 2:
                        if(parameters[1].ParameterType.IsArray){
                            return CSharpExpr.Call(method, first, CSharpExpr.NewArrayInit(
                                typeof(object),
                                args.Skip(1).Select(a => CSharpExpr.Call(expand_method, CSharpExpr.Convert(a, typeof(object))))
                            ));
                        }else{
                            var second_arg = CSharpExpr.Call(expand_method, CSharpExpr.Convert(args.ElementAt(1), typeof(object)));
                            return CSharpExpr.Call(method, first, second_arg);
                        }

                    case 3:
                        var second_arg2 = CSharpExpr.Call(expand_method, CSharpExpr.Convert(args.ElementAt(1), typeof(object)));
                        var third_arg = CSharpExpr.Call(expand_method, CSharpExpr.Convert(args.ElementAt(2), typeof(object)));
                        return CSharpExpr.Call(method, first, second_arg2, third_arg);

                    case 4:
                        var second_arg3 = CSharpExpr.Call(expand_method, CSharpExpr.Convert(args.ElementAt(1), typeof(object)));
                        var third_arg2 = CSharpExpr.Call(expand_method, CSharpExpr.Convert(args.ElementAt(2), typeof(object)));
                        var forth_arg = CSharpExpr.Call(expand_method, CSharpExpr.Convert(args.ElementAt(3), typeof(object)));
                        return CSharpExpr.Call(method, first, second_arg3, third_arg2, forth_arg);

                    default:
                        throw new InvalidOperationException("Unreachable");
                    }
                }else{
                    var builder = new StringBuilder("{0}");
                    for(int i = 1; i < args.Count(); ++i){
                        builder.Append(", ");
                        builder.Append("{" + i.ToString() + "}");
                    }

                    return CSharpExpr.Call(method, CSharpExpr.Constant(builder.ToString()), CSharpExpr.NewArrayInit(
                        typeof(string),
                        args.Select(a => CSharpExpr.Call(expand_method, CSharpExpr.Convert(a, typeof(object))))
                    ));
                }
            }else{
                if(method.ContainsGenericParameters){
                    var parameters = method.GetParameters();
                    var generic_param_indices = parameters.Where(p => p.ParameterType.IsGenericParameter)
                        .Select((p, index) => index);
                    var generic_types = args.Where((a, index) => generic_param_indices.Contains(index))
                        .Select(e => e.Type);
                    if(method.IsGenericMethod){
                        method = method.MakeGenericMethod(generic_types.ToArray());
                    }else{
                        var type = method.DeclaringType;
                        type = type.MakeGenericType(generic_types.ToArray());
                        method = type.GetMethod(method.Name);
                    }
                }

                var method_params = method.GetParameters();
                if(method_params.Length < args.Length){
                    // For varargs methods
                    int base_index = method_params.Length;
                    var array_param = method_params.Last();
                    if(!array_param.ParameterType.IsArray)
                        throw new EmitterException("Expected the last parameter is an array(params): {0}", array_param.Name);
                    
                    var array_type = array_param.ParameterType.GetElementType();
                    var array_creation = CSharpExpr.NewArrayInit(
                        array_type,
                        args.Skip(base_index - 1).Select(arg => CSharpExpr.Convert(arg, array_type))
                    );
                    var transformed_args = args.Take(base_index - 1).Concat(new []{array_creation});
                    args = transformed_args.ToArray();
                }else{
                    foreach(var pair in Enumerable.Range(0, args.Length).Zip(method.GetParameters(), (l, r) => new Tuple<int, ParameterInfo>(l, r))){
                        var arg = args[pair.Item1];
                        if(pair.Item2.ParameterType != arg.Type && !pair.Item2.ParameterType.IsByRef)
                            args[pair.Item1] = CSharpExpr.Convert(arg, pair.Item2.ParameterType);
                    }
                }
                return (inst != null) ? CSharpExpr.Call(inst, method, args) : CSharpExpr.Call(method, args);
            }
        }

        void DefineFunctionSignaturesAndFields(IEnumerable<EntityDeclaration> entities, EntityDeclaration startingPoint, CSharpEmitterContext context)
        {
            var tmp_counter = scope_counter;
            bool is_broken_out = false;
            var interface_definer = new InterfaceTypeDefiner(this, context);
            foreach(var entity in entities.SkipWhile(e => e != startingPoint)){
                if(entity is TypeDeclaration){
                    is_broken_out = true;
                    break;
                }

                if(entity is FunctionDeclaration 
                   && context.LazyTypeBuilder.GetMethod(entity.Name) == null)//context.LazyTypeBuilder.GetMethod(CSharpCompilerHelper.ConvertToPascalCase(entity.Name)) == null)
                    DefineFunctionSignature((FunctionDeclaration)entity, context);
                else if(entity is FieldDeclaration
                        && context.LazyTypeBuilder.GetField(entity.Name) == null)
                    DefineField((FieldDeclaration)entity, context);
            }

            if(!context.LazyTypeBuilder.IsInterfaceDefined && !is_broken_out)
                context.LazyTypeBuilder.CreateInterfaceType();
            
            scope_counter = tmp_counter;
        }

        void DefineFunctionSignature(FunctionDeclaration funcDecl, CSharpEmitterContext context)
        {
            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            var formal_parameters = funcDecl.Parameters.Select(param => VisitParameterDeclaration(param, context))
                                            .OfType<ExprTree.ParameterExpression>();

            var return_type = CSharpCompilerHelpers.GetNativeType(funcDecl.ReturnType);

            var is_global_function = !funcDecl.Modifiers.HasFlag(Modifiers.Public) && !funcDecl.Modifiers.HasFlag(Modifiers.Protected) && !funcDecl.Modifiers.HasFlag(Modifiers.Private);
            var self_param = CSharpExpr.Parameter(context.LazyTypeBuilder.InterfaceTypeBuilder, "self");
            var parameters = is_global_function ? formal_parameters : new []{self_param}.Concat(formal_parameters);

            var attrs = is_global_function ? MethodAttributes.Static :
                                                             funcDecl.Modifiers.HasFlag(Modifiers.Protected) ? MethodAttributes.Family :
                                                             funcDecl.Modifiers.HasFlag(Modifiers.Public) ? MethodAttributes.Public : MethodAttributes.Private;
            if(funcDecl.Modifiers.HasFlag(Modifiers.Export))
                attrs |= MethodAttributes.Public;
            else if(is_global_function)
                attrs |= MethodAttributes.Private;

            var param_types =
                from param in parameters
                select param.IsByRef ? param.Type.MakeByRefType() : param.Type;
            context.LazyTypeBuilder.DefineMethod(funcDecl.Name, attrs, return_type, param_types.ToArray());
            //context.LazyTypeBuilder.DefineMethod(CSharpCompilerHelper.ConvertToPascalCase(funcDecl.Name), attrs, return_type, param_types.ToArray());

            AscendScope();
            scope_counter = tmp_counter + 1;
        }

        void DefineField(FieldDeclaration fieldDecl, CSharpEmitterContext context)
        {
            FieldAttributes attr = FieldAttributes.Private | FieldAttributes.Static;

            // Don't set InitOnly flag or we'll fail to initialize the fields
            //if(fieldDecl.Modifiers.HasFlag(Modifiers.Immutable))
            //    attr |= FieldAttributes.InitOnly;

            if(fieldDecl.Modifiers.HasFlag(Modifiers.Export)){
                attr |= FieldAttributes.Public;
                attr ^= FieldAttributes.Private;
            }

            foreach(var init in fieldDecl.Initializers){
                if(init.Pattern is PatternWithType inner && inner.Pattern is IdentifierPattern ident_pat){
                    var type = CSharpCompilerHelpers.GetNativeType(ident_pat.Identifier.Type);
                    var field_builder = context.LazyTypeBuilder.DefineField(init.Name, type, !Expression.IsNullNode(init.Initializer), attr);
                    AddSymbol(ident_pat.Identifier, new ExpressoSymbol{PropertyOrField = field_builder});
                }else{
                    throw new EmitterException("Invalid module field!");
                }
            }
        }

        List<CSharpExpr> ExpandCollection(Type collectionType, ExprTree.ParameterExpression param, out List<ExprTree.ParameterExpression> parameters)
        {
            parameters = new List<ExprTree.ParameterExpression>();
            var out_parameters = parameters;
            if(collectionType.Name.StartsWith("Tuple", StringComparison.CurrentCulture)){
                var content = collectionType.GenericTypeArguments.Select((t, i) => {
                    var tmp_param = CSharpExpr.Parameter(t, "__" + VariableCount++);
                    out_parameters.Add(tmp_param);
                    return CSharpExpr.Assign(tmp_param, CSharpExpr.Property(param, "Item" + (i + 1)));
                }).OfType<CSharpExpr>()
                  .ToList();
                
                return content;
            }else{
                return new List<CSharpExpr>();
            }
        }

        ExprTree.ConditionalExpression ConstructConditionalExpressions(List<ExprTree.ConditionalExpression> exprs, int index)
        {
            if(index >= exprs.Count - 1)
                return exprs[index];

            var inner = ConstructConditionalExpressions(exprs, index + 1);
            if(inner == null)
                return exprs[index];
            else
                return CSharpExpr.IfThenElse(exprs[index].Test, exprs[index].IfTrue, inner);
        }
		#endregion
	}
}

