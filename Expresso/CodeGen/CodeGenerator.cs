﻿using System;
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
using ICSharpCode.NRefactory.PatternMatching;

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
    /// Expressoの構文木を解釈してReflection.Emitでコード生成をするクラス。
    /// It emitts IL codes from the AST of Expresso.
    /// Assumes we are all OK with syntax and semantics since this class is considered
    /// to belong to the backend part in compiler theory.
    /// </summary>
    /// <remarks>
    /// While emitting, we don't check the semantics and validity because the type check and
    /// other semantics analisys phases do that.
    /// We just care if an Expresso's expresson is convertible and has been converted to IL codes or not.
    /// </remarks>
    public partial class CodeGenerator : IAstWalker<CSharpEmitterContext, Type>
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
        static int VariableCount = 0;
		
        const string ClosureMethodName = "__Apply";
        const string ClosureDelegateName = "__ApplyMethod";
        const string HiddenMemberPrefix = "<>__";
        bool has_continue;

        Parser parser;
        SymbolTable symbol_table;
        ExpressoCompilerOptions options;
        MatchClauseIdentifierDefiner identifier_definer;
        ItemTypeInferencer item_type_inferencer;
        ILGenerator il_generator;
        GenericTypeParameterBuilder[] generic_types;

        List<Label> break_targets = new List<Label>();
        List<Label> continue_targets = new List<Label>();

        int scope_counter = 0;

        ExprTree.SymbolDocumentInfo document_info;

        /// <summary>
        /// Gets the generated assembly.
        /// </summary>
        public AssemblyBuilder AssemblyBuilder{
            get; private set;
        }

        public CodeGenerator(Parser parser, ExpressoCompilerOptions options)
        {
            this.parser = parser;
            symbol_table = parser.Symbols;
            this.options = options;
            identifier_definer = new MatchClauseIdentifierDefiner();
            item_type_inferencer = new ItemTypeInferencer(this);
            generic_types = new GenericTypeParameterBuilder[]{};

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
                throw new InvalidOperationException("An invalid identifier is invalid because it can't be used for any purpose.");
            
            try{
                Symbols.Add(ident.IdentifierId, symbol);
            }
            catch(ArgumentException){
                throw new InvalidOperationException(string.Format("The native symbol for '{0}' @ <id: {1}> is already added.", ident.Name, ident.IdentifierId));
            }
        }

        internal static void UpdateSymbol(Identifier ident, ExpressoSymbol symbol)
        {
            if(ident.IdentifierId == 0)
                throw new InvalidOperationException("An invalid identifier is invalid because it can't be used for any purpose.");

            if(!Symbols.TryGetValue(ident.IdentifierId, out var target_symbol))
                throw new InvalidOperationException(string.Format("The native symbol for '{0}' @ <id: {1}> isn't found.", ident.Name, ident.IdentifierId));

            // We don't need to update other fields
            //target_symbol.FieldBuilder = symbol.FieldBuilder;
            //target_symbol.Lambda = symbol.Lambda;
            //target_symbol.Member = symbol.Member;
            //target_symbol.Method = symbol.Method;
            //target_symbol.Parameter = symbol.Parameter;
            //target_symbol.Parameters = symbol.Parameters;
            target_symbol.PropertyOrField = symbol.PropertyOrField;
            //target_symbol.Type = symbol.Type;
            //target_symbol.TypeBuilder = symbol.TypeBuilder;
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

        static bool ContainsFunctionDefinitions(IEnumerable<EntityDeclaration> entities)
        {
            return entities.Any(e => e is FunctionDeclaration);
        }

        static Type[] ConvertToNativeTypes(AstNodeCollection<AstType> types)
        {
            var native_types = types.Select(t => CSharpCompilerHelpers.GetNativeType(t)).ToArray();
            if(native_types.Any(t => t == null))
                return Type.EmptyTypes;
            else
                return native_types;
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

        #region IAstWalker implementation

        public Type VisitAst(ExpressoAst ast, CSharpEmitterContext context)
        {
            if(context == null)
                context = new CSharpEmitterContext(){OperationTypeOnIdentifier = OperationType.Load, ParameterIndex = -1};
            
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
            var type_builder = ContainsFunctionDefinitions(ast.Declarations) ? new WrappedTypeBuilder(mod_builder, CSharpCompilerHelpers.ConvertToPascalCase(ast.Name),
                                                                                                      TypeAttributes.Class | TypeAttributes.Public, Enumerable.Empty<Type>(),
                                                                                                      true, false) : null;
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
                                                        (l, r) => new {Ast = l, Import = r})){
                Debug.Assert(pair.Import.ImportPaths.First().Name.StartsWith(pair.Ast.Name, StringComparison.CurrentCulture), "The module name must be matched to the import path");

                var external_module_count = context.CurrentModuleCount;
                var tmp_counter = scope_counter;
                VisitAst(pair.Ast, context);
                context.CurrentModuleCount = external_module_count;
                scope_counter = tmp_counter;

                var first_import = pair.Import.ImportPaths.First();
                if(!first_import.Name.Contains("::") && !first_import.Name.Contains(".")){
                    var first_alias = pair.Import.AliasTokens.First();
                    Symbols.Add(first_alias.IdentifierId, new ExpressoSymbol{
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

            //document_info = CSharpExpr.SymbolDocument(parser.scanner.FilePath, ExpressoCompilerHelpers.LanguageGuid);

            context.CustomAttributeSetter = asm_builder.SetCustomAttribute;
            context.AttributeTarget = AttributeTargets.Assembly;
            foreach(var attribute in ast.Attributes)
                VisitAttributeSection(attribute, context);

            context.CustomAttributeSetter = mod_builder.SetCustomAttribute;
            context.AttributeTarget = AttributeTargets.Module;
            foreach(var attribute in ast.Attributes)
                VisitAttributeSection(attribute, context);

            #if !NETCOREAPP2_0
            Console.WriteLine("Emitting code in {0}...", ast.ModuleName);
            #endif
            foreach(var import in ast.Imports)
                import.AcceptWalker(this, context);

            // Define only the function signatures so that the functions can call themselves
            // and the interface type is defined before we inspect the functions and fields
            DefineFunctionSignaturesAndFields(ast.Declarations, context);
            foreach(var decl in ast.Declarations){
                if(decl is TypeDeclaration){
                    ++scope_counter;
                    continue;
                }

                decl.AcceptWalker(this, context);
            }

            // If the module ends with a type we can't call CreateInterfaceType
            if(type_builder != null && !type_builder.IsDefined)
                type_builder.CreateInterfaceType();

            context.ExternalModuleType = (type_builder != null) ? type_builder.CreateType() : null;
            asm_builder.Save(file_name);

            AssemblyBuilder = asm_builder;

            return null;
        }

        public Type VisitBlock(BlockStatement block, CSharpEmitterContext context)
        {
            if(block.IsNull)
                return null;
            
            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            var parent_block = context.ContextAst;
            context.ContextAst = block;

            il_generator.BeginScope();

            var contents = new List<CSharpExpr>();
            foreach(var stmt in block.Statements)
                stmt.AcceptWalker(this, context);

            il_generator.EndScope();

            context.ContextAst = parent_block;

            /*var variables = ConvertSymbolsToParameters().ToList();
            if(block.Parent is CatchClause){
                var catch_clause = (CatchClause)block.Parent;
                var identifier = catch_clause.Identifier;
                variables = variables.Where(v => v.Name != identifier.Name)
                                     .Select(v => v)
                                     .ToList();
            }
            //if(context.ContextAst is FunctionDeclaration || context.ContextClosureLiteral != null && block.Parent is ClosureLiteralExpression)
            //    contents.Add(CSharpExpr.Label(ReturnTarget, DefaultReturnValue));*/

            AscendScope();
            scope_counter = tmp_counter + 1;

            return null;
        }

        public Type VisitBreakStatement(BreakStatement breakStmt, CSharpEmitterContext context)
        {
            int count = (int)breakStmt.CountExpression.Value;
            if(count > break_targets.Count)
                throw new EmitterException("Can not break out of loops that many times!");

            //break upto Count; => goto label;
            il_generator.Emit(OpCodes.Br_S, break_targets[break_targets.Count - count]);
            return null;// CSharpExpr.Break(break_targets[break_targets.Count - count]);
        }

        public Type VisitContinueStatement(ContinueStatement continueStmt, CSharpEmitterContext context)
        {
            int count = (int)continueStmt.CountExpression.Value;
            if(count > continue_targets.Count)
                throw new EmitterException("Can not break out of loops that many times!");

            //continue upto Count; => goto label;
            il_generator.Emit(OpCodes.Br_S, continue_targets[continue_targets.Count - count]);
            return null;//return CSharpExpr.Continue(continue_targets[continue_targets.Count - count]);
        }

        public Type VisitDoWhileStatement(DoWhileStatement doWhileStmt, CSharpEmitterContext context)
        {
            VisitBlock(doWhileStmt.Delegator.Body, context);
            VisitWhileStatement(doWhileStmt.Delegator, context);
                                                            //{ body;
            return null;                                    //  while_stmt}
        }

        public Type VisitEmptyStatement(EmptyStatement emptyStmt, CSharpEmitterContext context)
        {
            return null;
        }

        public Type VisitExpressionStatement(ExpressionStatement exprStmt, CSharpEmitterContext context)
        {
            exprStmt.Expression.AcceptWalker(this, context);
            return null;
        }

        public Type VisitForStatement(ForStatement forStmt, CSharpEmitterContext context)
        {
            //TODO: implement it
            /*int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            forStmt.Left.AcceptWalker(this, context);
            var iterator = forStmt.Target.AcceptWalker(this, context);

            var break_target = il_generator.DefineLabel();
            var continue_target = il_generator.DefineLabel();
            ++LoopCounter;
            break_targets.Add(break_target);
            continue_targets.Add(continue_target);

            var variables = ConvertSymbolsToParameters();
            var tmp = MakeEnumeratorCreations(iterator);
            var creation = tmp.Item1;
            var enumerator = tmp.Item2;
            var assignments = MakeIterableAssignments(variables, enumerator, break_target);

            var body = CSharpExpr.Block(variables,
                assignments.Concat(real_body.Expressions)
            );
            var loop = CSharpExpr.Loop(body, break_target, continue_target);
            il_generator.DeclareLocal();
            // Here, `Body` represents just the body block itself
            // In a for statement, we must move the iterator a step forward
            // and assign the result to inner-scope variables
            VisitBlock(forStmt.Body, context);

            break_targets.RemoveAt(break_targets.Count - 1);
            continue_targets.RemoveAt(continue_targets.Count - 1);
            AscendScope();
            scope_counter = tmp_counter + 1;

            var contents = new List<CSharpExpr>();
            contents.Add(creation);
            contents.Add(loop);*/
            return null;//CSharpExpr.Block(new []{enumerator}, contents);
        }

        public Type VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStatement, CSharpEmitterContext context)
        {
            // TODO: implement it 
            /*int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            var parent_params = context.AdditionalParameters;
            context.AdditionalParameters = new List<ExprTree.ParameterExpression>();
            valueBindingForStatement.Initializer.Pattern.AcceptWalker(this, context);
            var bound_variables = context.AdditionalParameters;
            context.AdditionalParameters = parent_params;
            //foreach(var variable in valueBindingForStatement.Pattern){
            //    var bound_variable = CSharpExpr.Variable(CSharpCompilerHelper.GetNativeType(variable.NameToken.Type), variable.Name);
            //    AddSymbol(variable.NameToken, new ExpressoSymbol{Parameter = bound_variable});
            //    bound_variables.Add(bound_variable);
            //    iterators.Add(variable.Initializer.AcceptWalker(this, context));
            //}
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
            contents.Add(loop);*/
            return null; //CSharpExpr.Block(new []{enumerator}, contents);
            // for let x in some_sequence {} => (enumerator){ creation (loop_variables, block_variables){ loop_variable_initializer { real_body } } }
        }

        public Type VisitIfStatement(IfStatement ifStmt, CSharpEmitterContext context)
        {
            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            var jump_label = il_generator.DefineLabel();
            var false_label = il_generator.DefineLabel();
            var prev_op_type = context.OperationTypeOnIdentifier;
            context.OperationTypeOnIdentifier = OperationType.Load;
            ifStmt.Condition.AcceptWalker(this, context);
            context.OperationTypeOnIdentifier = prev_op_type;
            il_generator.Emit(OpCodes.Brfalse_S, false_label);

            VisitBlock(ifStmt.TrueBlock, context);
            if(ifStmt.FalseStatement.IsNull){
                il_generator.MarkLabel(jump_label);
                il_generator.MarkLabel(false_label);

                AscendScope();
                scope_counter = tmp_counter + 1;

                return null;
            }else{
                il_generator.Emit(OpCodes.Br_S, jump_label);
                il_generator.MarkLabel(false_label);

                ifStmt.FalseStatement.AcceptWalker(this, context);

                il_generator.MarkLabel(jump_label);

                AscendScope();
                scope_counter = tmp_counter + 1;
                return null;
            }
        }

        public Type VisitReturnStatement(ReturnStatement returnStmt, CSharpEmitterContext context)
        {
            // If we are in the main function, then make return statements do nothing
            if(!CanReturn(returnStmt))
                return null;

            returnStmt.Expression.AcceptWalker(this, context);
            il_generator.Emit(OpCodes.Ret);
            return null;
        }

        public Type VisitMatchStatement(MatchStatement matchStmt, CSharpEmitterContext context)
        {
            //TODO : implement it
            // Match statement semantics: First we evaluate the target expression
            // and assign the result to a temporary variable that's alive within the whole statement.
            // All the pattern clauses must meet the same condition.
            // If context.ContextExpression is an ExprTree.ConditionalExpression
            // we know that we're at least at the second branch.
            // If it is null, then we're at the first branch so just set it to the context expression.
            /*var target = matchStmt.Target.AcceptWalker(this, context);
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
                //exprs.Add(tmp);
            }

            /*var res = ConstructConditionalExpressions(exprs, 0);

            var target_var_assignment = CSharpExpr.Assign(target_var, target);
            block_contents.Insert(0, target_var_assignment);
            block_contents.Add(res);
            parameters.Add(target_var);

            AscendScope();
            scope_counter = tmp_counter + 1;

            context.TemporaryVariable = null;
            context.ContextAst = context_ast;*/

            return null;//CSharpExpr.Block(parameters, block_contents);
        }

        public Type VisitThrowStatement(ThrowStatement throwStmt, CSharpEmitterContext context)
        {
            VisitObjectCreationExpression(throwStmt.CreationExpression, context);
            il_generator.ThrowException(context.TargetType);
            return null;
        }

        public Type VisitTryStatement(TryStatement tryStmt, CSharpEmitterContext context)
        {
            // TODO: implement it
            var body_block = VisitBlock(tryStmt.EnclosingBlock, context);
            var catches = new List<ExprTree.CatchBlock>();
            foreach(var @catch in tryStmt.CatchClauses){
                VisitCatchClause(@catch, context);
                catches.Add(context.CatchBlock);
            }

            var finally_clause = tryStmt.FinallyClause.AcceptWalker(this, context);
            if(finally_clause == null)
                return null;//CSharpExpr.TryCatch(body_block, catches.ToArray());
            else if(!catches.Any())
                return null;//CSharpExpr.TryFinally(body_block, finally_clause);
            else
                return null;//CSharpExpr.TryCatchFinally(body_block, finally_clause, catches.ToArray());
        }

        public Type VisitWhileStatement(WhileStatement whileStmt, CSharpEmitterContext context)
        {
            var end_loop = il_generator.DefineLabel();
            var loop_start = il_generator.DefineLabel();
            ++LoopCounter;
            break_targets.Add(end_loop);
            continue_targets.Add(loop_start);

            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            if(!(whileStmt.Parent is DoWhileStatement))
                il_generator.Emit(OpCodes.Br_S, loop_start);
            
            VisitBlock(whileStmt.Body, context);

            il_generator.MarkLabel(loop_start);
            whileStmt.Condition.AcceptWalker(this, context);
            il_generator.Emit(OpCodes.Brtrue_S, loop_start);
            
            break_targets.RemoveAt(break_targets.Count - 1);
            continue_targets.RemoveAt(continue_targets.Count - 1);

            il_generator.MarkLabel(end_loop);

            AscendScope();
            scope_counter = tmp_counter + 1;

            return null;
        }

        public Type VisitYieldStatement(YieldStatement yieldStmt, CSharpEmitterContext context)
        {
            throw new NotImplementedException();
        }

        public Type VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl, CSharpEmitterContext context)
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

        public Type VisitAssignment(AssignmentExpression assignment, CSharpEmitterContext context)
        {
            Type result = null;
            if(assignment.Left is SequenceExpression lhs){    // see if `assignment` is a simultaneous assignment
                // expected form: a, b, ... = 1, 2, ...
                // evaluation order: we will see the left-hand-side and right-hand-side as a pair and evaluate them from left to right
                // that is, they will be evaluated as a = 1, b = 2, ...
                var rhs = (SequenceExpression)assignment.Right;
                context.RequestPropertyOrField = true;
                foreach(var operands in lhs.Items.Zip(rhs.Items, (l, r) => new {Lhs = l, Rhs = r})){
                    if(assignment.Operator == OperatorType.Assign)
                        context.OperationTypeOnIdentifier = OperationType.None;
                    
                    var tmp = operands.Lhs.AcceptWalker(this, context);
                    if(result == null)
                        result = tmp;
                    
                    context.OperationTypeOnIdentifier = OperationType.Load;
                    operands.Rhs.AcceptWalker(this, context);

                    switch(assignment.Operator){
                    case OperatorType.Plus:
                        il_generator.Emit(OpCodes.Add);
                        break;

                    case OperatorType.Minus:
                        il_generator.Emit(OpCodes.Sub);
                        break;

                    case OperatorType.Times:
                        il_generator.Emit(OpCodes.Mul);
                        break;

                    case OperatorType.Divide:
                        il_generator.Emit(OpCodes.Div);
                        break;

                    case OperatorType.Modulus:
                        il_generator.Emit(OpCodes.Rem);
                        break;

                    case OperatorType.Power:
                        var pow_method = typeof(Math).GetMethod("Pow");
                        il_generator.Emit(OpCodes.Call, pow_method);
                        break;
                    }

                    EmitSet((FieldInfo)context.PropertyOrField, context.TargetLocalBuilder, result);
                }
            }else{
                // falls into composition branch
                // expected form: a = b = ...
                context.RequestPropertyOrField = true;
                context.PropertyOrField = null;
                assignment.Right.AcceptWalker(this, context);
                result = assignment.Left.AcceptWalker(this, context);

                EmitSet((FieldInfo)context.PropertyOrField, context.TargetLocalBuilder, result);
            }

            context.RequestPropertyOrField = false;
            context.PropertyOrField = null;
            context.TargetLocalBuilder = null;

            return result;
        }

        public Type VisitBinaryExpression(BinaryExpression binaryExpr, CSharpEmitterContext context)
        {
            context.RequestPropertyOrField = true;

            var result = binaryExpr.Left.AcceptWalker(this, context);
            binaryExpr.Right.AcceptWalker(this, context);

            var label = il_generator.DefineLabel();

            context.RequestPropertyOrField = false;
            context.PropertyOrField = null;
            EmitBinaryOp(binaryExpr.Operator, ref label);
            il_generator.MarkLabel(label);

            return result;
        }

        public Type VisitCallExpression(CallExpression call, CSharpEmitterContext context)
        {
            var parent_args = context.ArgumentTypes;
            context.ArgumentTypes = ConvertToNativeTypes(call.OverloadSignature.Parameters);

            context.RequestMethod = true;
            call.Target.AcceptWalker(this, context);
            context.RequestMethod = false;
            context.ArgumentTypes = parent_args;

            var method = context.Method;
            EmitCallExpression(method, call, context);
            var result = method.ReturnType;
            context.Method = null;

            return result;
        }

        public Type VisitCastExpression(CastExpression castExpr, CSharpEmitterContext context)
        {
            var original_type = castExpr.Target.AcceptWalker(this, context);
            var to_type = CSharpCompilerHelpers.GetNativeType(castExpr.ToExpression);
            EmitCast(original_type, to_type);

            return to_type;
        }

        public Type VisitCatchClause(CatchClause catchClause, CSharpEmitterContext context)
        {
            // TODO: implement it
            /*var ident = catchClause.Identifier;
            var exception_type = CSharpCompilerHelpers.GetNativeType((ident.Type.IdentifierNode.Type != null) ? ident.Type.IdentifierNode.Type : ident.Type);
            var param = CSharpExpr.Parameter(exception_type, ident.Name);
            AddSymbol(ident, new ExpressoSymbol{Parameter = param});

            var body = VisitBlock(catchClause.Body, context);
            context.CatchBlock = CSharpExpr.Catch(param, body);*/
            return null;
        }

        public Type VisitClosureLiteralExpression(ClosureLiteralExpression closure, CSharpEmitterContext context)
        {
            //TODO: impelement it
            /*var prev_additionals = context.Additionals;
            context.Additionals = new List<object>();

            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            var closure_type_builder = new LazyTypeBuilder(context.ModuleBuilder, HiddenMemberPrefix + "Closure`" + ClosureId++, TypeAttributes.Class, Enumerable.Empty<Type>(), false, false);

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
            scope_counter = tmp_counter + 1;*/

            return null;
        }

        public Type VisitComprehensionExpression(ComprehensionExpression comp, CSharpEmitterContext context)
        {
            //TODO: implement it
            var generator = comp.Item.AcceptWalker(this, context);
            //context.ContextExpression = generator;
            var type = comp.Body.AcceptWalker(this, context);
            return type;
        }

        public Type VisitComprehensionForClause(ComprehensionForClause compFor, CSharpEmitterContext context)
        {
            compFor.Left.AcceptWalker(this, context);
            compFor.Target.AcceptWalker(this, context);
            compFor.Body.AcceptWalker(this, context);
            return null;
        }

        public Type VisitComprehensionIfClause(ComprehensionIfClause compIf, CSharpEmitterContext context)
        {
            // TODO: implement it
            /*if(compIf.Body.IsNull)      //[generator...if Condition] -> ...if(Condition) seq.Add(generator);
                return CSharpExpr.IfThen(compIf.Condition.AcceptWalker(this, context), context.ContextExpression);
            else                        //[...if Condition...] -> ...if(Condition){...}
                return CSharpExpr.IfThen(compIf.Condition.AcceptWalker(this, context), compIf.Body.AcceptWalker(this, context));
                */
            return null;
        }

        public Type VisitConditionalExpression(ConditionalExpression condExpr, CSharpEmitterContext context)
        {
            var false_label = il_generator.DefineLabel();
            var end_label = il_generator.DefineLabel();

            condExpr.Condition.AcceptWalker(this, context);
            il_generator.Emit(OpCodes.Brfalse_S, false_label);

            var result = condExpr.TrueExpression.AcceptWalker(this, context);
            il_generator.Emit(OpCodes.Br_S, end_label);
            il_generator.MarkLabel(false_label);

            condExpr.FalseExpression.AcceptWalker(this, context);
            il_generator.MarkLabel(end_label);

            return result;
        }

        public Type VisitFinallyClause(FinallyClause finallyClause, CSharpEmitterContext context)
        {
            VisitBlock(finallyClause.Body, context);
            return null;
        }

        public Type VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue, CSharpEmitterContext context)
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

                var type = keyValue.ValueExpression.AcceptWalker(this, context);
                return type;
            }else{
                // In a dictionary literal, the key can be any expression that is evaluated
                // to a hashable object.
                keyValue.KeyExpression.AcceptWalker(this, context);
                keyValue.ValueExpression.AcceptWalker(this, context);
                return null;
            }
        }

        public Type VisitLiteralExpression(LiteralExpression literal, CSharpEmitterContext context)
        {
            EmitObject(literal.Value);
            return literal.Value.GetType();
        }

        public Type VisitIdentifier(Identifier ident, CSharpEmitterContext context)
        {
            //if(context.ContextClosureLiteral != null && context.ContextClosureLiteral.LiftedIdentifiers.Any(i => i.Name == ident.Name))
            //    return CSharpExpr.Field(context.ParameterSelf, ident.Name);

            var symbol = GetRuntimeSymbol(ident);
            if(symbol != null){
                if(symbol.LocalBuilder != null){
                    if(context.RequestType)
                        context.TargetType = symbol.LocalBuilder.LocalType;

                    if(context.OperationTypeOnIdentifier == OperationType.Load)
                        il_generator.Emit(OpCodes.Ldloc, symbol.LocalBuilder);

                    return symbol.LocalBuilder.LocalType;
                }else if(symbol.Parameter != null){
                    if(context.RequestType)
                        context.TargetType = symbol.Parameter.Type;

                    if(context.OperationTypeOnIdentifier == OperationType.Load && symbol.ParameterIndex != -1)
                        LoadArg(symbol.ParameterIndex);

                    return symbol.Parameter.Type;
                }else if(context.RequestPropertyOrField && symbol.PropertyOrField != null){
                    context.PropertyOrField = symbol.PropertyOrField;
                    return (symbol.PropertyOrField is PropertyInfo) ? ((PropertyInfo)symbol.PropertyOrField).PropertyType : ((FieldInfo)symbol.PropertyOrField).FieldType;
                }else if(context.RequestType && symbol.Type != null){
                    context.TargetType = symbol.Type;
                    if(context.TargetType.GetConstructors().Any()){
                        // context.TargetType could be a static type
                        context.Constructor = context.TargetType.GetConstructors().Last();
                    }

                    return symbol.Type;
                }else if(context.RequestMethod){
                    if(symbol.Method == null)
                        throw new EmitterException("The native symbol '{0}' isn't defined.", ident.Name);

                    context.Method = symbol.Method;
                    return null;
                }else{
                    throw new EmitterException("I can't guess what you want.");
                }
            }else{
                if(context.TargetType != null && context.TargetType.IsEnum){
                    var enum_field = context.TargetType.GetField(ident.Name);
                    context.PropertyOrField = enum_field ?? throw new EmitterException("It is found that the native symbol '{0}' doesn't represent an enum field.", ident.Name);
                    AddSymbol(ident, new ExpressoSymbol{PropertyOrField = enum_field});
                    return null;
                }else if(context.TargetType != null && context.RequestMethod){
                    // For methods or functions in external modules
                    // We regard types containing namespaces as types from other assemblies
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

        public Type VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq, CSharpEmitterContext context)
        {
            var intseq_ctor = typeof(ExpressoIntegerSequence).GetConstructor(new []{typeof(int), typeof(int), typeof(int), typeof(bool)});
       
            intSeq.Start.AcceptWalker(this, context);
            intSeq.End.AcceptWalker(this, context);
            intSeq.Step.AcceptWalker(this, context);
            EmitObject(intSeq.UpperInclusive);

            if(context.RequestMethod && context.Method == null)
                context.Method = typeof(ExpressoIntegerSequence).GetMethod("Includes");

            il_generator.Emit(OpCodes.Newobj, intseq_ctor);
            return typeof(ExpressoIntegerSequence);      //new ExpressoIntegerSequence(Start, End, Step, UpperInclusive)
        }

        public Type VisitIndexerExpression(IndexerExpression indexExpr, CSharpEmitterContext context)
        {
            var target_type = indexExpr.Target.AcceptWalker(this, context);
            var arg_types = indexExpr.Arguments.Select(a => a.AcceptWalker(this, context));

            if(arg_types.Count() == 1 && arg_types.First().Name == "ExpressoIntegerSequence"){
                var seq_type = target_type;
                var elem_type = seq_type.IsArray ? seq_type.GetElementType() : seq_type.GenericTypeArguments[0];
                var slice_type = typeof(Slice<,>).MakeGenericType(new []{seq_type, elem_type});
                var ctor = slice_type.GetConstructor(new []{seq_type, typeof(ExpressoIntegerSequence)});

                il_generator.Emit(OpCodes.Newobj, ctor);
                return slice_type;   // a[ExpressoIntegerSequence]
            }

            var type = target_type;
            // We don't need to set here because Assignment calls this only when it should read values
            if(type.IsArray){
                EmitLoadElem(target_type);
                return target_type.GetElementType();
            }else{
                var property_info = type.GetProperty("Item");
                il_generator.Emit(OpCodes.Callvirt, property_info.GetMethod);
                return property_info.GetMethod.ReturnType;
            }
        }

        public Type VisitMemberReference(MemberReferenceExpression memRef, CSharpEmitterContext context)
        {
            // In Expresso, a member access can be resolved either to a field reference or an (instance and static) method call
            var prev_op_type = context.OperationTypeOnIdentifier;
            context.OperationTypeOnIdentifier = OperationType.Load;
            var expr = memRef.Target.AcceptWalker(this, context);
            context.OperationTypeOnIdentifier = prev_op_type;
            context.RequestPropertyOrField = true;
            context.RequestMethod = true;

            VisitIdentifier(memRef.Member, context);
            context.RequestPropertyOrField = false;
            context.RequestMethod = false;

            if(context.Method != null){
                return null;    // Parent should be a CallExpression
            }else if(context.PropertyOrField is PropertyInfo property){
                context.PropertyOrField = null;
                il_generator.Emit(OpCodes.Callvirt, property.GetMethod);
                return property.GetMethod.ReturnType;
            }else{
                var field = (FieldInfo)context.PropertyOrField;
                context.PropertyOrField = null;

                EmitLoadField(field);
                return field.FieldType;
            }
        }

        public Type VisitPathExpression(PathExpression pathExpr, CSharpEmitterContext context)
        {
            if(pathExpr.Items.Count == 1){
                context.RequestType = true;
                context.RequestMethod = true;
                context.RequestPropertyOrField = true;

                var type = VisitIdentifier(pathExpr.AsIdentifier, context);
                context.RequestType = false;
                context.RequestMethod = false;
                context.RequestPropertyOrField = false;

                // Assume m is a module variable and it's for let b = m;
                if(context.PropertyOrField != null && context.PropertyOrField is FieldInfo field){
                    il_generator.Emit(OpCodes.Ldsfld, field);
                    return field.FieldType;
                }else{
                    return type;
                }
            }

            context.TargetType = null;
            // We do this because the items in a path should already be resolved
            // and a path with more than 1 item can only refer to an external module's method
            // or an enum member
            var last_item = pathExpr.Items.Last();
            var native_symbol = GetRuntimeSymbol(last_item);
            if(native_symbol != null){
                context.TargetType = native_symbol.Type;
                context.Method = native_symbol.Method;
                context.PropertyOrField = native_symbol.PropertyOrField;
            }else{
                throw new EmitterException("It is found that the runtime symbol '{0}' doesn't represent anything.", last_item.Name);
            }

            if(native_symbol.PropertyOrField != null && native_symbol.PropertyOrField is FieldInfo field2){
                var first_item = pathExpr.Items.First();
                var type_table = symbol_table.GetTypeTable(first_item.Name);
                if(type_table != null && type_table.TypeKind == ClassType.Enum && pathExpr.Ancestors.Any(a => a is VariableInitializer)){
                    // This code is needed for assgining raw value enums
                    var field_type = field2.FieldType;
                    var wrapper_type = field_type.DeclaringType;
                    var ctor = wrapper_type.GetConstructors().First();
                    il_generator.Emit(OpCodes.Ldsfld, field2);
                    il_generator.Emit(OpCodes.Newobj, ctor);
                    return wrapper_type;
                }else{
                    il_generator.Emit(OpCodes.Ldsfld, field2);
                    return field2.FieldType;
                }
            }
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

            return null;
        }

        public Type VisitParenthesizedExpression(ParenthesizedExpression parensExpr, CSharpEmitterContext context)
        {
            return parensExpr.Expression.AcceptWalker(this, context);
        }

        public Type VisitObjectCreationExpression(ObjectCreationExpression creation, CSharpEmitterContext context)
        {
            context.TargetType = null;
            creation.TypePath.AcceptWalker(this, context);
            if(creation.CtorType.IsNull){
                // Construct an enum variant
                var variant_name = ((MemberType)creation.TypePath).ChildType.Name;
                var field_info = context.TargetType.GetField(HiddenMemberPrefix + variant_name);

                var field_type = field_info.FieldType;
                var type = field_type.GenericTypeArguments.Any() ? typeof(Tuple) : typeof(Unit);
                var create_method = type.GetMethods()
                                        .Where(m => m.Name == "Create" && m.GetParameters().Length == field_type.GenericTypeArguments.Length)
                                        .First();
                create_method = create_method.IsGenericMethod ? create_method.MakeGenericMethod(field_type.GenericTypeArguments) : create_method;

                var ctor = context.TargetType.GetConstructors().First();
                ctor.GetParameters().Select<ParameterInfo, Unit>(p => {
                    if(p.ParameterType != field_type){
                        il_generator.Emit(OpCodes.Ldnull);
                        EmitCast(typeof(object), p.ParameterType);
                        return null;
                    }else{
                        context.RequestPropertyOrField = true;
                        creation.Items.Select(i => i.AcceptWalker(this, context));
                        il_generator.Emit(OpCodes.Call, create_method);
                        return null;
                    }
                });
                il_generator.Emit(OpCodes.Newobj, ctor);
                
                return context.TargetType;
            }

            // Don't report TargetType missing error because it was already reported in TypeChecker
            //if(context.TargetType == null)
            //    throw new EmitterException("")
            var arg_types =
                from p in creation.CtorType.Parameters
                                  select CSharpCompilerHelpers.GetNativeType(p);
            context.Constructor = context.TargetType.GetConstructor(arg_types.ToArray());
            if(context.Constructor == null){
                throw new EmitterException(
                    "No constructors found for the path `{0}` with arguments {1}",
                    creation,
                    creation.TypePath, CSharpCompilerHelpers.ExpandContainer(arg_types.ToArray())
                );
            }

            var formal_params = context.Constructor.GetParameters();
            var args = new CSharpExpr[creation.Items.Count];
            context.RequestPropertyOrField = true;
            // TODO: make object creation arguments pair to constructor parameters
            foreach(var item in creation.Items){
                item.AcceptWalker(this, context);

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
            }

            il_generator.Emit(OpCodes.Newobj, context.Constructor);
            return context.Constructor.DeclaringType;
        }

        public Type VisitSequenceInitializer(SequenceInitializer seqInitializer, CSharpEmitterContext context)
        {
            var obj_type = seqInitializer.ObjectType;
            var seq_type = CSharpCompilerHelpers.GetContainerType(obj_type);
            // If this node represents a dictionary literal
            // context.Constructor will get set the appropriate constructor method.
            context.Constructor = null;
            var prev_additionals = context.Additionals;
            context.Additionals = new List<object>();
            if(seq_type == typeof(Array)){
                var elem_type = CSharpCompilerHelpers.GetNativeType(obj_type.TypeArguments.First());
                if(elem_type == typeof(ExpressoIntegerSequence)){
                    foreach(var item in seqInitializer.Items)
                        item.AcceptWalker(this, context);
                    
                    var array_create_method = typeof(ExpressoIntegerSequence).GetMethod("CreateArrayFromIntSeq");
                    il_generator.Emit(OpCodes.Call, array_create_method);
                    return typeof(int[]);
                }else{
                    EmitNewArray(elem_type, seqInitializer.Items, item => {
                        item.AcceptWalker(this, context);
                    });
                    return elem_type.MakeArrayType();
                }
            }else if(seq_type == typeof(List<>)){
                var elem_type = CSharpCompilerHelpers.GetNativeType(obj_type.TypeArguments.First());
                if(elem_type == typeof(ExpressoIntegerSequence)){
                    foreach(var item in seqInitializer.Items)
                        item.AcceptWalker(this, context);
                    
                    var list_create_method = typeof(ExpressoIntegerSequence).GetMethod("CreateListFromIntSeq");
                    il_generator.Emit(OpCodes.Call, list_create_method);
                    return typeof(List<int>);
                }else{
                    var generic_type = EmitListInitForList(elem_type, seqInitializer.Items, context);
                    return generic_type;
                }
            }else if(seq_type == typeof(Dictionary<,>)){
                var key_type = CSharpCompilerHelpers.GetNativeType(obj_type.TypeArguments.FirstOrNullObject());
                var value_type = CSharpCompilerHelpers.GetNativeType(obj_type.TypeArguments.LastOrNullObject());

                var generic_type = EmitListInitForDictionary(key_type, value_type, seqInitializer.Items.OfType<KeyValueLikeExpression>(), context);
                return generic_type;
            }else if(seq_type == typeof(Tuple)){
                var child_types = new List<Type>();
                foreach(var item in seqInitializer.Items){
                    var tmp = item.AcceptWalker(this, context);
                    child_types.Add(tmp);
                }
                var ctor_method = typeof(Tuple).GetMethod("Create", child_types.ToArray());
                il_generator.Emit(OpCodes.Call, ctor_method);
                return CSharpCompilerHelpers.GuessTupleType(child_types);
            }else{
                throw new EmitterException("Can not emit code.");
            }
        }

        public Type VisitMatchClause(MatchPatternClause matchClause, CSharpEmitterContext context)
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
            // TODO: implement it
            /*int tmp_counter = scope_counter;
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

            //if(destructuring_exprs.Count() != context.Additionals.Count()){
                // The number of destructured variables must match in every pattern
            //    throw new EmitterException(
            //       "Expected the pattern contains {0} variables, but it only contains {1}!",
            //        destructuring_exprs.Count(), context.Additionals.Count()
            //    );
            //}

            var guard = matchClause.Guard.AcceptWalker(this, context);
            if(guard != null)
                res = CSharpExpr.AndAlso(res, guard);

            AscendScope();
            scope_counter = tmp_counter + 1;

            return (res == null) ? null : CSharpExpr.IfThen(res, context.ContextExpression);*/
            return null;
        }

        public Type VisitSequenceExpression(SequenceExpression seqExpr, CSharpEmitterContext context)
        {
            // A sequence expression is always translated to a tuple
            var types = new List<Type>();
            foreach(var item in seqExpr.Items){
                var tmp = item.AcceptWalker(this, context);
                types.Add(tmp);
            }

            if(types.Count == 1){
                return types.First();
            }else{
                var ctor_method = typeof(Tuple).GetGenericMethod("Create", BindingFlags.Public | BindingFlags.Static, types.ToArray());

                il_generator.Emit(OpCodes.Call, ctor_method);
                return CSharpCompilerHelpers.GuessTupleType(types);
            }
        }

        public Type VisitUnaryExpression(UnaryExpression unaryExpr, CSharpEmitterContext context)
        {
            var type = unaryExpr.Operand.AcceptWalker(this, context);
            EmitUnaryOp(unaryExpr.Operator);
            return type;
        }

        public Type VisitSelfReferenceExpression(SelfReferenceExpression selfRef, CSharpEmitterContext context)
        {
            LoadArg(0);
            return CSharpCompilerHelpers.GetNativeType(selfRef.SelfIdentifier.Type);
        }

        public Type VisitSuperReferenceExpression(SuperReferenceExpression superRef, CSharpEmitterContext context)
        {
            // TODO: implement it
            //var super_type = context.LazyTypeBuilder.BaseType;
            //return CSharpExpr.Parameter(super_type, "super");
            return null;
        }

        public Type VisitNullReferenceExpression(NullReferenceExpression nullRef, CSharpEmitterContext context)
        {
            il_generator.Emit(OpCodes.Ldnull);
            return typeof(object);
        }

        public Type VisitCommentNode(CommentNode comment, CSharpEmitterContext context)
        {
            // Just ignore comment nodes...
            return null;
        }

        public Type VisitTextNode(TextNode textNode, CSharpEmitterContext context)
        {
            // Just ignore text nodes, too...
            return null;
        }

        public Type VisitTypeConstraint(TypeConstraint constraint, CSharpEmitterContext context)
        {
            return null;
        }

        // AstType nodes should be treated with special care
        public Type VisitSimpleType(SimpleType simpleType, CSharpEmitterContext context)
        {
            // Use case: Called from ObjectCreationExpression
            var symbol = GetRuntimeSymbol(simpleType.IdentifierToken);
            if(symbol != null){
                context.TargetType = symbol.Type;
                // For enum member
                context.PropertyOrField = symbol.PropertyOrField;
                return symbol.Type;
            }else{
                var type = CSharpCompilerHelpers.GetNativeType(simpleType);
                context.TargetType = type;
                return type;
            }
        }

        public Type VisitPrimitiveType(PrimitiveType primitiveType, CSharpEmitterContext context)
        {
            return null;
        }

        public Type VisitReferenceType(ReferenceType referenceType, CSharpEmitterContext context)
        {
            return null;
        }

        public Type VisitMemberType(MemberType memberType, CSharpEmitterContext context)
        {
            context.RequestType = true;
            // When this MemerType refers to an enum variant we should only look into Target
            var type_table = symbol_table.GetTypeTable(memberType.Target.Name);
            memberType.Target.AcceptWalker(this, context);
            if(type_table != null && type_table.TypeKind == ClassType.Enum && context.TargetType != null){
                context.RequestType = false;
                return context.TargetType;
            }

            VisitSimpleType(memberType.ChildType, context);
            context.RequestType = false;
            return context.TargetType;
        }

        public Type VisitFunctionType(FunctionType funcType, CSharpEmitterContext context)
        {
            return null;
        }

        public Type VisitParameterType(ParameterType paramType, CSharpEmitterContext context)
        {
            return null;
        }

        public Type VisitPlaceholderType(PlaceholderType placeholderType, CSharpEmitterContext context)
        {
            return null;
        }

        public Type VisitKeyValueType(KeyValueType keyValueType, CSharpEmitterContext context)
        {
            return null;
        }

        public Type VisitAttributeSection(AttributeSection section, CSharpEmitterContext context)
        {
            // TODO: implement it
            /*var target_context = context.AttributeTarget;

            var attributes = section.Attributes.Select(attribute => {
                var creation = (ExprTree.NewExpression)VisitObjectCreationExpression(attribute, context);
                var args = creation.Arguments
                                   .Select(attr => {
                    if(attr is ExprTree.ConstantExpression constant)
                        return constant.Value;
                    else if(attr is ExprTree.MemberExpression member)
                        return ((FieldInfo)member.Member).GetValue(null);
                    else
                        throw new InvalidOperationException("Unknown argument expression!");
                }).ToArray();
                return new {Ctor = creation.Constructor, Arguments = args};
            });
               
            IEnumerable<AttributeTargets> allowed_targets;
            AttributeTargets preferable_target;
            string help_message;
            switch(section.Parent){
            case ExpressoAst ast:
                allowed_targets = new []{AttributeTargets.Assembly, AttributeTargets.Module};
                preferable_target = AttributeTargets.Module;
                help_message = "'assembly' or 'module'";
                break;

            case TypeDeclaration type_decl:
                allowed_targets = new []{AttributeTargets.Class, AttributeTargets.Enum};
                preferable_target = (type_decl.TypeKind == ClassType.Class) ? AttributeTargets.Class :
                (type_decl.TypeKind == ClassType.Enum) ? AttributeTargets.Enum : AttributeTargets.Property;
                help_message = "'type'";
                break;

            case FieldDeclaration field_decl:
                allowed_targets = new []{AttributeTargets.Field};
                preferable_target = AttributeTargets.Field;
                help_message = "'field'";
                break;

            case FunctionDeclaration func_decl:
                allowed_targets = new []{AttributeTargets.Method, AttributeTargets.ReturnValue};
                preferable_target = AttributeTargets.Method;
                help_message = "'method'";
                break;

            case ParameterDeclaration param_decl:
                allowed_targets = new []{AttributeTargets.Parameter};
                preferable_target = AttributeTargets.Parameter;
                help_message = "'param'";
                break;

            default:
                throw new InvalidOperationException("Unreachable");
            }

            if(!section.AttributeTargetToken.IsNull){
                var specified_context = AttributeSection.GetAttributeTargets(section.AttributeTarget);
                if(!allowed_targets.Any(t => t == specified_context)){
                    throw new ParserException(
                        "The attribute target '{0}' isn't expected in this context.",
                        "ES4021",
                        section
                    ){
                        HelpObject = help_message
                    };
                }

                if(target_context.HasFlag(specified_context)){
                    foreach(var attribute in attributes)
                        context.CustomAttributeSetter(new CustomAttributeBuilder(attribute.Ctor, attribute.Arguments));
                }
            }else{
                if(target_context.HasFlag(preferable_target)){
                    foreach(var attribute in attributes)
                        context.CustomAttributeSetter(new CustomAttributeBuilder(attribute.Ctor, attribute.Arguments));
                }
            }*/

            return null;
        }

        public Type VisitAliasDeclaration(AliasDeclaration aliasDecl, CSharpEmitterContext context)
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

        public Type VisitImportDeclaration(ImportDeclaration importDecl, CSharpEmitterContext context)
        {
            foreach(var pair in importDecl.ImportPaths.Zip(importDecl.AliasTokens, (l, r) => new {ImportPath = l, Alias = r})){
                if(!pair.ImportPath.Name.Contains("::") && !pair.ImportPath.Name.Contains("."))
                    break;
                
                var import_path = pair.ImportPath;
                var alias = pair.Alias;

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
                        var member_name = type_name;

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

        public Type VisitFunctionDeclaration(FunctionDeclaration funcDecl, CSharpEmitterContext context)
        {
            if(funcDecl.Body.IsNull)
                return null;
            
            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            var prev_context_ast = context.ContextAst;
            context.ContextAst = funcDecl;

            var is_global_function = !funcDecl.Modifiers.HasFlag(Modifiers.Public) && !funcDecl.Modifiers.HasFlag(Modifiers.Protected) && !funcDecl.Modifiers.HasFlag(Modifiers.Private);
            var flags = is_global_function ? BindingFlags.Static : BindingFlags.Instance;
            if(funcDecl.Modifiers.HasFlag(Modifiers.Export) || funcDecl.Modifiers.HasFlag(Modifiers.Public))
                flags |= BindingFlags.Public;
            else
                flags |= BindingFlags.NonPublic;

            var interface_func = context.LazyTypeBuilder.GetInterfaceMethod(funcDecl.Name, flags);
            AddSymbol(funcDecl.NameToken, new ExpressoSymbol{Method = interface_func});

            var method_builder = context.LazyTypeBuilder.GetMethodBuilder(funcDecl.Name);
            var prev_il_generator = il_generator;
            il_generator = method_builder.GetILGenerator();

            funcDecl.Body.AcceptWalker(this, context);
            context.ContextAst = prev_context_ast;

            if(!(funcDecl.Body.Statements.Last() is ReturnStatement))
                il_generator.Emit(OpCodes.Ret);

            if(funcDecl.Name == "main")
                context.AssemblyBuilder.SetEntryPoint(interface_func);
            
            il_generator = prev_il_generator;

            AscendScope();
            scope_counter = tmp_counter + 1;

            return null;
        }

        public Type VisitTypeDeclaration(TypeDeclaration typeDecl, CSharpEmitterContext context)
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

                if(typeDecl.TypeKind != ClassType.Interface)
                    context.LazyTypeBuilder.CreateType();
            }
            finally{
                context.LazyTypeBuilder = parent_type;
            }

            AscendScope();
            scope_counter = original_count + 1;
            return null;
        }

        public Type VisitFieldDeclaration(FieldDeclaration fieldDecl, CSharpEmitterContext context)
        {
            foreach(var init in fieldDecl.Initializers){
                var field_builder = (init.NameToken != null) ? Symbols[init.NameToken.IdentifierId].FieldBuilder : throw new EmitterException("Invalid field: {0}", init.Pattern);
                var prev_il_generator = il_generator;
                il_generator = context.LazyTypeBuilder.GetILGeneratorForFieldInit(field_builder);
                init.Initializer.AcceptWalker(this, context);

                il_generator = prev_il_generator;
            }

            return null;
        }

        public Type VisitParameterDeclaration(ParameterDeclaration parameterDecl, CSharpEmitterContext context)
        {
            Type type = null;
            if(!Symbols.ContainsKey(parameterDecl.NameToken.IdentifierId)){
                var native_type = RetrieveType(parameterDecl.ReturnType);
                var param = CSharpExpr.Parameter(native_type, parameterDecl.Name);
                AddSymbol(parameterDecl.NameToken, new ExpressoSymbol{Parameter = param, ParameterIndex = context.ParameterIndex});
                type = native_type;
            }else{
                throw new InvalidOperationException("Symbols already contains the parameter " + parameterDecl.Name);
                //param = (ExprTree.ParameterExpression)VisitIdentifier(parameterDecl.NameToken, context);
            }

            return type;
        }

        public Type VisitVariableInitializer(VariableInitializer initializer, CSharpEmitterContext context)
        {
            if(initializer.Initializer.IsNull){
                var prev_op_type = context.OperationTypeOnIdentifier;
                context.OperationTypeOnIdentifier = OperationType.None;
                initializer.Pattern.AcceptWalker(this, context);
                context.OperationTypeOnIdentifier = prev_op_type;
                return null;
            }else{
                if(initializer.Pattern.Pattern is TuplePattern tuple_pattern){
                    if(initializer.Initializer is SequenceExpression seq_expr){
                        // let (t1, t2) = Tuple.Create(...);
                        var prev_op_type = context.OperationTypeOnIdentifier;
                        var types = new List<Type>();
                        foreach(var pair in tuple_pattern.Patterns.Zip(seq_expr.Items, (l, r) => new {Pattern = l, Expression = r})){
                            context.OperationTypeOnIdentifier = OperationType.Load;
                            var tmp = pair.Expression.AcceptWalker(this, context);
                            types.Add(tmp);

                            context.OperationTypeOnIdentifier = OperationType.Set;
                            pair.Pattern.AcceptWalker(this, context);
                        }

                        return CSharpCompilerHelpers.GuessTupleType(types);
                    }else if(initializer.Initializer is PathExpression path){
                        // let (t1, t2) = t where t is Tuple
                        var type = initializer.Initializer.AcceptWalker(this, context);
                        var prev_op_type = context.OperationTypeOnIdentifier;
                        context.OperationTypeOnIdentifier = OperationType.Set;

                        foreach(var pair in Enumerable.Range(1, tuple_pattern.Patterns.Count).Zip(tuple_pattern.Patterns, (l, r) => new {Index = l, Pattern = r})){
                            il_generator.Emit(OpCodes.Dup);
                            var property = type.GetProperty("Item" + pair.Index);
                            il_generator.Emit(OpCodes.Callvirt, property.GetMethod);
                            pair.Pattern.AcceptWalker(this, context);
                        }
                        il_generator.Emit(OpCodes.Pop);

                        //var debug_info_list = debug_infos.ToList();
                        //debug_info_list.AddRange(tmps);
                        //result = CSharpExpr.Block(debug_info_list);//options.BuildType.HasFlag(BuildType.Debug) ? CSharpExpr.Block(debug_info_list) : CSharpExpr.Block(tmps);
                        return type;
                    }else{
                        throw new InvalidOperationException("Invalid expression found: " + initializer.Initializer.GetType().Name);
                    }
                }else{
                    var prev_op_type = context.OperationTypeOnIdentifier;
                    context.OperationTypeOnIdentifier = OperationType.Load;
                    var type = initializer.Initializer.AcceptWalker(this, context);

                    context.OperationTypeOnIdentifier = OperationType.Set;
                    initializer.Pattern.AcceptWalker(this, context);
                    context.OperationTypeOnIdentifier = prev_op_type;
                    /*if(options.BuildType.HasFlag(BuildType.Debug)){
                        var debug_info_list = debug_infos.ToList();
                        debug_info_list.Add(assignment);
                        result = CSharpExpr.Block(debug_info_list);
                    }else{
                        result = assignment;
                    }*/
                    return type;
                }
            }
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
        public Type VisitWildcardPattern(WildcardPattern wildcardPattern, CSharpEmitterContext context)
        {
            // A wildcard pattern is translated to the else clause
            // so just return null to indicate that.
            return null;
        }

        public Type VisitIdentifierPattern(IdentifierPattern identifierPattern, CSharpEmitterContext context)
        {
            // An identifier pattern can arise by itself or as a child
            Type type = null;
            if(!(context.ContextAst is MatchStatement)){
                type = RetrieveType(identifierPattern.Identifier.Type);
                var local_builder = il_generator.DeclareLocal(type);
                AddSymbol(identifierPattern.Identifier, new ExpressoSymbol{LocalBuilder = local_builder});

                if(context.OperationTypeOnIdentifier == OperationType.Set)
                    EmitSet(null, local_builder, null);
                
                var start_loc = identifierPattern.Identifier.StartLocation;
                var end_loc = identifierPattern.Identifier.EndLocation;
                //il_generator.MarkSequencePoint();
                //var debug_info = CSharpExpr.DebugInfo(document_info, start_loc.Line, start_loc.Column, end_loc.Line, end_loc.Column);
            }else{
                type = VisitIdentifier(identifierPattern.Identifier, context);
            }

            /*if(identifierPattern.Parent is MatchPatternClause){
                // Set context.ContextExpression to a block
                //var native_type = CSharpCompilerHelper.GetNativeType(identifierPattern.Identifier.Type);
                var param = GetRuntimeSymbol(identifierPattern.Identifier).Parameter;
                var assignment = CSharpExpr.Assign(param, context.TemporaryVariable);
                context.ContextExpression = CSharpExpr.Block(new []{type}, new []{assignment, context.ContextExpression});
            }

            if(context.PropertyOrField == null && context.TargetType != null && context.ContextAst is MatchStatement 
               && identifierPattern.Parent is DestructuringPattern destructuring && !destructuring.IsEnum){
                // context.TargetType is supposed to be set in CSharpEmitter.VisitIdentifier
                var field = context.TargetType.GetField(identifierPattern.Identifier.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if(field == null){
                    throw new EmitterException(
                        "The type `{0}` doesn't have the field `{1}`.",
                        context.TargetType, identifierPattern.Identifier.Name
                    );
                }
                context.PropertyOrField = field;
            }*/

            if(!identifierPattern.InnerPattern.IsNull)
                return identifierPattern.InnerPattern.AcceptWalker(this, context);
            else
                return type;
        }

        public Type VisitCollectionPattern(CollectionPattern collectionPattern, CSharpEmitterContext context)
        {
            // First, make type validation expression
            // TODO: implement it
            /*var collection_type = CSharpCompilerHelpers.GetContainerType(collectionPattern.CollectionType);
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
                        var block_contents = context.Additionals.OfType<CSharpExpr>().ToList();
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

            return res;//CSharpExpr.TypeIs(context.TemporaryVariable, collection_type);*/
            return null;
        }

        public Type VisitDestructuringPattern(DestructuringPattern destructuringPattern, CSharpEmitterContext context)
        {
            // TODO: implement it
            /*context.TargetType = null;
            destructuringPattern.TypePath.AcceptWalker(this, context);

            var type = context.TargetType;
            if(destructuringPattern.IsEnum){
                var variant_name = ((MemberType)destructuringPattern.TypePath).ChildType.Name;
                context.TemporaryExpression = CSharpExpr.Field(context.TemporaryVariable, HiddenMemberPrefix + variant_name);
            }

            context.RequestPropertyOrField = true;
            var prev_additionals = context.Additionals;
            context.Additionals = new List<object>();
            var prev_additional_params = context.AdditionalParameters;
            context.AdditionalParameters = new List<ExprTree.ParameterExpression>();

            CSharpExpr res = null;
            var block = new List<CSharpExpr>();
            var block_params = new List<ExprTree.ParameterExpression>();
            var i = 1;
            foreach(var pattern in destructuringPattern.Items){
                var item_ast_type = pattern.AcceptWalker(item_type_inferencer);
                if(item_ast_type == null)
                    continue;
                
                var item_type = CSharpCompilerHelpers.GetNativeType(item_ast_type);
                //var tmp_param = CSharpExpr.Parameter(item_type, "__" + VariableCount++);

                //var prev_tmp_variable = context.TemporaryVariable;
                //context.TemporaryVariable = tmp_param;
                CSharpExpr prev_tmp_expr = null;
                if(destructuringPattern.IsEnum){
                    var field_name = "Item" + i++;
                    var field_access = CSharpExpr.PropertyOrField(context.TemporaryExpression, field_name);
                    prev_tmp_expr = context.TemporaryExpression;
                    context.TemporaryExpression = field_access;
                }
                context.PropertyOrField = null;
                var expr = pattern.AcceptWalker(this, context);
                //context.TemporaryVariable = prev_tmp_variable;

                if(destructuringPattern.IsEnum){
                    var param = (ExprTree.ParameterExpression)expr;
                    var assignment2 = CSharpExpr.Assign(param, context.TemporaryExpression);
                    block_params.Add(param);
                    block.Add(assignment2);

                    context.TemporaryExpression = prev_tmp_expr;
                }else{
                    var field_access = CSharpExpr.Field(context.TemporaryExpression, (FieldInfo)context.PropertyOrField);
                    //var assignment = CSharpExpr.Assign(tmp_param, field_access);
                    //context.Additionals.Add(assignment);
                    //context.AdditionalParameters.Add(tmp_param);
                    //block.Add(assignment);
                    //block_params.Add(tmp_param);

                    var param = expr as ExprTree.ParameterExpression;
                    if(param != null){
                        var assignment2 = CSharpExpr.Assign(param, field_access);
                        block_params.Add(param);
                        block.Add(assignment2);
                    }else{
                        if(context.Additionals.Any()){
                            var block_contents = context.Additionals.OfType<CSharpExpr>().ToList();
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
            }

            if(res == null){
                if(destructuringPattern.IsEnum){
                    var variant_name = ((MemberType)destructuringPattern.TypePath).ChildType.Name;
                    var field_access = CSharpExpr.Field(context.TemporaryVariable, HiddenMemberPrefix + variant_name);
                    res = CSharpExpr.NotEqual(field_access, CSharpExpr.Constant(null));
                }else{
                    var native_type = CSharpCompilerHelpers.GetNativeType(destructuringPattern.TypePath);
                    res = CSharpExpr.TypeIs(context.TemporaryVariable, native_type);
                }
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

            return res;//CSharpExpr.TypeIs(context.TemporaryVariable, type);*/
            return null;
        }

        public Type VisitTuplePattern(TuplePattern tuplePattern, CSharpEmitterContext context)
        {
            // Tuple patterns should always be combined with value binding patterns
            // TODO: implement it
            /*var prev_additionals = context.Additionals;
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
                        var block_contents = context.Additionals.OfType<CSharpExpr>().ToList();
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

            return res;//CSharpExpr.TypeIs(context.TemporaryVariable, tuple_type);*/
            return null;
        }

        public Type VisitExpressionPattern(ExpressionPattern exprPattern, CSharpEmitterContext context)
        {
            // Common scinario in an expression pattern:
            // An integer sequence expression or a literal expression.
            // In the former case we should test an integer against an IntSeq type object using an IntSeq's method
            // while in the latter case we should just test the value against the literal
            // TODO: implement it
            context.RequestMethod = true;
            context.Method = null;
            var type = exprPattern.Expression.AcceptWalker(this, context);
            context.RequestMethod = false;

            if(context.Method != null && context.Method.DeclaringType.Name == "ExpressoIntegerSequence"){
                il_generator.Emit(OpCodes.Callvirt, context.Method);
                return null;
            }else if(context.ContextAst is MatchStatement){
                il_generator.Emit(OpCodes.Ceq);
                return null;
            }else{
                return type;
            }
        }

        public Type VisitIgnoringRestPattern(IgnoringRestPattern restPattern, CSharpEmitterContext context)
        {
            return null;
        }

        public Type VisitKeyValuePattern(KeyValuePattern keyValuePattern, CSharpEmitterContext context)
        {
            // TODO: implement it
            /*context.PropertyOrField = null;
            context.RequestPropertyOrField = true;
            VisitIdentifier(keyValuePattern.KeyIdentifier, context);
            var expr = keyValuePattern.Value.AcceptWalker(this, context);

            var param = expr as ExprTree.ParameterExpression;
            if(param != null){
                var assignment = CSharpExpr.Assign(param, CSharpExpr.Field(context.TemporaryVariable, (FieldInfo)context.PropertyOrField));
                return assignment;
            }
            return expr;*/
            return null;
        }

        public Type VisitPatternWithType(PatternWithType pattern, CSharpEmitterContext context)
        {
            return pattern.Pattern.AcceptWalker(this, context);
        }

        public Type VisitTypePathPattern(TypePathPattern pathPattern, CSharpEmitterContext context)
        {
            // TODO: implement it
            // We can't just delegate it to VisitMemberType because we can't distinguish between tuple style enums and raw value style enums
            /*var member = (MemberType)pathPattern.TypePath;
            context.RequestType = true;
            member.Target.AcceptWalker(this, context);
            context.RequestType = false;

            // We can't just delegate to VisitSimpleType because it doesn't take properties or fields into account
            context.RequestMethod = true;
            context.RequestPropertyOrField = true;
            VisitIdentifier(member.ChildType.IdentifierNode, context);
            context.RequestMethod = false;
            context.RequestPropertyOrField = false;

            var field_access = CSharpExpr.Field(null, (FieldInfo)context.PropertyOrField);
            return CSharpExpr.Equal(context.TemporaryVariable, field_access);*/
            return null;
        }

        public Type VisitNullNode(AstNode nullNode, CSharpEmitterContext context)
        {
            // Just ignore null nodes...
            return null;
        }

        public Type VisitNewLine(NewLineNode newlineNode, CSharpEmitterContext context)
        {
            // Just ignore new lines...
            return null;
        }

        public Type VisitWhitespace(WhitespaceNode whitespaceNode, CSharpEmitterContext context)
        {
            // Just ignore whitespaces...
            return null;
        }

        public Type VisitExpressoTokenNode(ExpressoTokenNode tokenNode, CSharpEmitterContext context)
        {
            // It doesn't matter what tokens Expresso uses
            return null;
        }

        public Type VisitPatternPlaceholder(AstNode placeholder, ICSharpCode.NRefactory.PatternMatching.Pattern child, CSharpEmitterContext context)
        {
            // Ignore placeholder nodes because they are just placeholders...
            return null;
        }

        #endregion

		#region methods
        void EmitBinaryOp(OperatorType opType, ref Label label)
		{
			switch(opType){
			case OperatorType.BitwiseAnd:
                il_generator.Emit(OpCodes.And);
                break;

			case OperatorType.BitwiseShiftLeft:
                il_generator.Emit(OpCodes.Shl);
                break;

			case OperatorType.BitwiseOr:
                il_generator.Emit(OpCodes.Or);
                break;

			case OperatorType.BitwiseShiftRight:
                il_generator.Emit(OpCodes.Shr);
                break;

            case OperatorType.ConditionalAnd:
            case OperatorType.ConditionalOr:
                // nothing to emit
                break;

			case OperatorType.ExclusiveOr:
                il_generator.Emit(OpCodes.Xor);
                break;

			case OperatorType.Divide:
                il_generator.Emit(OpCodes.Div);
                break;

			case OperatorType.Equality:
                il_generator.Emit(OpCodes.Ceq);
                break;

			case OperatorType.GreaterThan:
                il_generator.Emit(OpCodes.Cgt);
                //il_generator.Emit(OpCodes.Ldc_I4_1);
                //il_generator.Emit(OpCodes.Ceq);
                break;

            case OperatorType.GreaterThanOrEqual:
                il_generator.Emit(OpCodes.Clt);
                il_generator.Emit(OpCodes.Ldc_I4_0);
                il_generator.Emit(OpCodes.Ceq);
                break;

            case OperatorType.LessThan:
                il_generator.Emit(OpCodes.Clt);
                //il_generator.Emit(OpCodes.Ldc_I4_1);
                //il_generator.Emit(OpCodes.Ceq);
                break;

            case OperatorType.LessThanOrEqual:
                il_generator.Emit(OpCodes.Cgt);
                il_generator.Emit(OpCodes.Ldc_I4_0);
                il_generator.Emit(OpCodes.Ceq);
                break;

			case OperatorType.Minus:
                il_generator.Emit(OpCodes.Sub);
                break;

			case OperatorType.Modulus:
                il_generator.Emit(OpCodes.Rem);
                break;

			case OperatorType.InEquality:
                il_generator.Emit(OpCodes.Ceq);
                il_generator.Emit(OpCodes.Not);
                break;

			case OperatorType.Plus:
                il_generator.Emit(OpCodes.Add);
                break;

			case OperatorType.Power:
                var pow_method = typeof(Math);
                il_generator.Emit(OpCodes.Call, pow_method);
                break;

			case OperatorType.Times:
                il_generator.Emit(OpCodes.Mul);
                break;

			default:
				throw new EmitterException("Unknown binary operator!");
			}
		}

        void EmitUnaryOp(OperatorType opType)
		{
			switch(opType){
            case OperatorType.Reference:
                // The parameter modifier "ref" is the primary candidate for this operator and it is expressed as a type
                // So we don't need to do anything here :-)
                break;

			case OperatorType.Plus:
                break;

			case OperatorType.Minus:
                il_generator.Emit(OpCodes.Neg);
                break;

			case OperatorType.Not:
                il_generator.Emit(OpCodes.Ldc_I4_0);
                il_generator.Emit(OpCodes.Ceq);
                break;

			default:
				throw new EmitterException("Unknown unary operator!");
			}
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

        void EmitCallExpression(MethodInfo method, CallExpression call, CSharpEmitterContext context)
        {
            if(method == null)
                throw new ArgumentNullException(nameof(method));

            if(method.DeclaringType.Name == "Console" && (method.Name == "Write" || method.Name == "WriteLine") ||
                     method.DeclaringType.Name == "String" && method.Name == "Format"){
                var first = call.Arguments.First();
                var expand_method = typeof(CSharpCompilerHelpers).GetMethod("ExpandContainer");
                if(first is LiteralExpression first_string && first_string.Value is string && ((string)first_string.Value).Contains("{0}")){
                    var parameters = method.GetParameters();
                    first_string.AcceptWalker(this, context);

                    switch(parameters.Length){
                    case 2:
                        if(parameters[1].ParameterType.IsArray){
                            EmitNewArray(parameters[1].ParameterType.GetElementType(), call.Arguments.Skip(1), arg => {
                                var original_type = arg.AcceptWalker(this, context);
                                EmitCast(original_type, typeof(object));
                                il_generator.Emit(OpCodes.Call, expand_method);
                            });
                            il_generator.Emit(OpCodes.Call, method);
                        }else{
                            var original_type = call.Arguments.ElementAt(1).AcceptWalker(this, context);
                            EmitCast(original_type, typeof(object));
                            il_generator.Emit(OpCodes.Call, expand_method);
                            il_generator.Emit(OpCodes.Call, method);
                        }
                        break;

                    case 3:
                    case 4:
                        foreach(var arg in call.Arguments.Skip(1)){
                            var original_type = arg.AcceptWalker(this, context);
                            EmitCast(original_type, typeof(object));
                            il_generator.Emit(OpCodes.Call, expand_method);
                        }

                        il_generator.Emit(OpCodes.Call, method);
                        break;

                    default:
                        throw new InvalidOperationException("Unreachable");
                    }
                }else{
                    var builder = new StringBuilder("{0}");
                    for(int i = 1; i < call.Arguments.Count; ++i){
                        builder.Append(", ");
                        builder.Append("{" + i.ToString() + "}");
                    }

                    il_generator.Emit(OpCodes.Ldstr, builder.ToString());

                    var prev_op_type = context.OperationTypeOnIdentifier;
                    context.OperationTypeOnIdentifier = OperationType.Load;
                    EmitNewArray(typeof(string), call.Arguments, arg => {
                        var original_type = arg.AcceptWalker(this, context);
                        EmitCast(original_type, typeof(object));
                        il_generator.Emit(OpCodes.Call, expand_method);
                    });
                    context.OperationTypeOnIdentifier = prev_op_type;
                    il_generator.Emit(OpCodes.Call, method);
                }
            }else{
                if(method.IsGenericMethod){//method.ContainsGenericParameters){
                    var parameters = method.GetParameters();
                    var generic_param_names = parameters.Where(p => p.ParameterType.IsGenericParameter)
                                                        .Select(p => p.Name);
                    var method_generic_types = call.TypeArguments.Where(ta => generic_param_names.Contains(ta.Name))
                                                   .Select(e => CSharpCompilerHelpers.GetNativeType(e));
                    if(method.IsGenericMethod){
                        method = method.MakeGenericMethod(method_generic_types.ToArray());
                    }else{
                        var type = method.DeclaringType;
                        type = type.MakeGenericType(method_generic_types.ToArray());
                        method = type.GetMethod(method.Name);
                    }
                }

                var method_params = method.GetParameters();
                if(method_params.Length < call.Arguments.Count){
                    // For varargs methods
                    int base_index = method_params.Length;
                    var array_param = method_params.Last();
                    if(!array_param.ParameterType.IsArray)
                        throw new EmitterException("Expected the last parameter is an array(params): {0}", array_param.Name);
                    
                    var array_type = array_param.ParameterType.GetElementType();
                    foreach(var arg in call.Arguments)
                        arg.AcceptWalker(this, context);

                    foreach(var arg in call.Arguments.Skip(base_index - 1)){
                        var original_type = arg.AcceptWalker(this, context);
                        EmitCast(original_type, array_type);
                    }
                }else if(method_params.Length > call.Arguments.Count){
                    // For optional parameters
                    foreach(var arg in call.Arguments)
                        arg.AcceptWalker(this, context);
                    
                    foreach(var i in Enumerable.Range(call.Arguments.Count, method_params.Length - call.Arguments.Count)){
                        if(!method_params[i].HasDefaultValue)
                            throw new InvalidOperationException(string.Format("Expected #{0} of the parameters of {1} has default value.", i, method.Name));

                        EmitObject(method_params[i].RawDefaultValue);
                    }
                }else{
                    foreach(var pair in Enumerable.Range(0, call.Arguments.Count).Zip(method.GetParameters(), (l, r) => new {Index = l, Param = r})){
                        var arg = call.Arguments.ElementAt(pair.Index);
                        arg.AcceptWalker(this, context);
                        //if(pair.Param.ParameterType != arg.Type && !pair.Param.ParameterType.IsByRef)
                        //    call[pair.Item1] = CSharpExpr.Convert(arg, pair.Item2.ParameterType);
                    }
                }

                EmitCall(method);
            }
        }

        void DefineFunctionSignaturesAndFields(IEnumerable<EntityDeclaration> entities, CSharpEmitterContext context)
        {
            // We can't make this method an iterator because then we can't look at all the entities
            // We need to store scope_counter here because the DefineFunctionSignature method will make it 1 step forward every time it will be called
            var tmp_counter = scope_counter;
            foreach(var entity in entities){
                if(entity is TypeDeclaration type_decl){
                    type_decl.AcceptWalker(this, context);
                    continue;
                }
                
                if(entity is FunctionDeclaration func_decl
                   && context.LazyTypeBuilder.GetMethodBuilder(func_decl.Name) == null)
                    DefineFunctionSignature(func_decl, context);
                else if(entity is FieldDeclaration field_decl
                        && context.LazyTypeBuilder.GetFieldBuilder(field_decl.Initializers.First().Name) == null)
                    DefineField(field_decl, context);
            }

            if(context.LazyTypeBuilder != null)
                context.LazyTypeBuilder.CreateInterfaceType();

            scope_counter = tmp_counter;
        }

        void DefineFunctionSignature(FunctionDeclaration funcDecl, CSharpEmitterContext context)
        {
            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            var param_types = funcDecl.Parameters.Select((param, index) => {
                context.ParameterIndex = index;
                return VisitParameterDeclaration(param, context);
            });
            var return_type = CSharpCompilerHelpers.GetNativeType(funcDecl.ReturnType);

            var is_global_function = !funcDecl.Modifiers.HasFlag(Modifiers.Public) && !funcDecl.Modifiers.HasFlag(Modifiers.Protected) && !funcDecl.Modifiers.HasFlag(Modifiers.Private);
            var flags = is_global_function ? MethodAttributes.Static :
                                                             funcDecl.Modifiers.HasFlag(Modifiers.Protected) ? MethodAttributes.Family :
                                                             funcDecl.Modifiers.HasFlag(Modifiers.Public) ? MethodAttributes.Public : MethodAttributes.Private;
            if(funcDecl.Modifiers.HasFlag(Modifiers.Export))
                flags |= MethodAttributes.Public;
            else if(is_global_function)
                flags |= MethodAttributes.Private;

            var method_builder = context.LazyTypeBuilder.DefineMethod(funcDecl.Name, flags, return_type, param_types.ToArray(), this, context, funcDecl);

            /*context.CustomAttributeSetter = method_builder.SetCustomAttribute;
            context.AttributeTarget = AttributeTargets.Method;
            // We don't call VisitAttributeSection directly so that we can avoid unnecessary method calls
            funcDecl.Attribute.AcceptWalker(this, context);*/

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

                    /*context.CustomAttributeSetter = field_builder.SetCustomAttribute;
                    context.AttributeTarget = AttributeTargets.Field;
                    // We don't call VisitAttributeSection directly so that we can avoid unnecessary method calls
                    fieldDecl.Attribute.AcceptWalker(this, context);*/

                    // PropertyOrField is needed so that we can refer to it easily later on
                    // We can do this because we wouldn't need to be able to return module classes
                    AddSymbol(ident_pat.Identifier, new ExpressoSymbol{FieldBuilder = field_builder, PropertyOrField = field_builder});
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

        PropertyInfo GetTupleProperty(IdentifierPattern identifierPattern, Type tupleType)
        {
            var parent = identifierPattern.Parent;
            int i = 0;
            if(parent is DestructuringPattern destructuring){
                destructuring.Items.Any(item => {
                    if(item.IsMatch(identifierPattern)){
                        return true;
                    }else{
                        ++i;
                        return false;
                    }
                });
            }else if(parent is TuplePattern tuple){
                tuple.Patterns.Any(p => {
                    if(p.IsMatch(identifierPattern)){
                        return true;
                    }else{
                        ++i;
                        return false;
                    }
                });
            }

            return tupleType.GetProperty("Item" + i);
        }

        Type RetrieveType(AstType astType)
        {
            var generic_type_candidates = generic_types.Where(gt => gt.Name == astType.Name);
            if(generic_type_candidates.Any())
                return generic_type_candidates.First();
            else    
                return CSharpCompilerHelpers.GetNativeType(astType);
        }

        #region Emit methods
        void EmitObject(object obj)
        {
            if(obj is string str){
                il_generator.Emit(OpCodes.Ldstr, str);
            }else if(obj is char ch){
                EmitInt(ch);
            }else if(obj is int integer){
                EmitInt(integer);
            }else if(obj is uint unsigned){
                EmitUInt(unsigned);
            }else if(obj is float f){
                il_generator.Emit(OpCodes.Ldc_R4, f);
            }else if(obj is double d){
                il_generator.Emit(OpCodes.Ldc_R8, d);
            }else if(obj is bool b){
                il_generator.Emit(b ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            }else if(obj is BigInteger big_int){
                var parse_method = typeof(BigInteger).GetMethod("Parse", new []{typeof(string)});
                il_generator.Emit(OpCodes.Ldstr, big_int.ToString());
                il_generator.Emit(OpCodes.Call, parse_method);
            }else{
                throw new ArgumentException(string.Format("Unknown object type!: {0}", obj.GetType().Name));
            }
        }

        void EmitCast(Type originalType, Type type)
        {
            if(type == typeof(object) && !originalType.IsClass)
                il_generator.Emit(OpCodes.Box, originalType);
            else if(type == typeof(byte))
                il_generator.Emit(OpCodes.Conv_U1);
            else if(type == typeof(int))
                il_generator.Emit(OpCodes.Conv_I4);
            else if(type == typeof(uint))
                il_generator.Emit(OpCodes.Conv_U4);
            else if(type == typeof(float))
                il_generator.Emit(OpCodes.Conv_R4);
            else if(type == typeof(double))
                il_generator.Emit(OpCodes.Conv_R8);
            else
                il_generator.Emit(OpCodes.Castclass, type);
        }

        void EmitCall(MethodInfo method)
        {
            if(method.IsStatic)
                il_generator.Emit(OpCodes.Call, method);
            else
                il_generator.Emit(OpCodes.Callvirt, method);
        }

        void EmitLoad(FieldInfo field, LocalBuilder localBuilder, Type targetType)
        {
            if(localBuilder != null)
                il_generator.Emit(OpCodes.Ldloc, localBuilder);
            else if(field != null)
                EmitLoadField(field);
            else
                EmitLoadElem(targetType);
        }

        void EmitLoadField(FieldInfo field)
        {
            if(field.IsStatic)
                il_generator.Emit(OpCodes.Ldsfld, field);
            else
                il_generator.Emit(OpCodes.Ldfld, field);
        }

        void EmitSet(FieldInfo field, LocalBuilder localBuilder, Type targetType)
        {
            if(localBuilder != null)
                il_generator.Emit(OpCodes.Stloc, localBuilder);
            else if(field != null)
                EmitSetField(field);
            else
                EmitSetElem(targetType);
        }

        void EmitSetField(FieldInfo field)
        {
            if(field.IsStatic)
                il_generator.Emit(OpCodes.Stsfld, field);
            else
                il_generator.Emit(OpCodes.Stfld, field);
        }

        void EmitNewArray(Type elementType, IEnumerable<Expression> initializers, Action<Expression> action)
        {
            var count = initializers.Count();
            EmitInt(count);
            il_generator.Emit(OpCodes.Newarr, elementType);
            foreach(var pair in Enumerable.Range(0, count).Zip(initializers, (l, r) => new {Index = l, Initializer = r})){
                il_generator.Emit(OpCodes.Dup);
                EmitInt(pair.Index);
                action(pair.Initializer);
                EmitSetElem(elementType);
            }
        }

        void EmitInt(int i)
        {
            switch(i){
            case -1:
                il_generator.Emit(OpCodes.Ldc_I4_M1);
                break;

            case 0:
                il_generator.Emit(OpCodes.Ldc_I4_0);
                break;

            case 1:
                il_generator.Emit(OpCodes.Ldc_I4_1);
                break;

            case 2:
                il_generator.Emit(OpCodes.Ldc_I4_2);
                break;

            case 3:
                il_generator.Emit(OpCodes.Ldc_I4_3);
                break;

            case 4:
                il_generator.Emit(OpCodes.Ldc_I4_4);
                break;

            case 5:
                il_generator.Emit(OpCodes.Ldc_I4_5);
                break;

            case 6:
                il_generator.Emit(OpCodes.Ldc_I4_6);
                break;

            case 7:
                il_generator.Emit(OpCodes.Ldc_I4_7);
                break;

            case 8:
                il_generator.Emit(OpCodes.Ldc_I4_8);
                break;

            default:
                if(i <= sbyte.MaxValue)
                    il_generator.Emit(OpCodes.Ldc_I4_S, (sbyte)i);
                else
                    il_generator.Emit(OpCodes.Ldc_I4, i);
                
                break;
            }
        }

        void EmitUInt(uint n)
        {
            if(n <= sbyte.MaxValue)
                il_generator.Emit(OpCodes.Ldc_I4, n);
            else
                EmitInt((int)n);

            il_generator.Emit(OpCodes.Conv_I4);
        }

        void LoadArg(int i)
        {
            switch(i){
            case 0:
                il_generator.Emit(OpCodes.Ldarg_0);
                break;

            case 1:
                il_generator.Emit(OpCodes.Ldarg_1);
                break;

            case 2:
                il_generator.Emit(OpCodes.Ldarg_2);
                break;

            case 3:
                il_generator.Emit(OpCodes.Ldarg_3);
                break;

            default:
                if(i <= short.MaxValue)
                    il_generator.Emit(OpCodes.Ldarg_S, (short)i);
                else
                    il_generator.Emit(OpCodes.Ldarg, i);

                break;
            }
        }

        Type EmitListInitForList(Type elementType, IEnumerable<Expression> initializers, CSharpEmitterContext context)
        {
            var list_type = typeof(List<>);
            var generic_type = list_type.MakeGenericType(elementType);
            var add_method = generic_type.GetMethod("Add");
            var ctor = generic_type.GetConstructor(new Type[]{});

            var prev_op_type = context.OperationTypeOnIdentifier;
            context.OperationTypeOnIdentifier = OperationType.Load;
            il_generator.Emit(OpCodes.Newobj, ctor);
            foreach(var initializer in initializers){
                il_generator.Emit(OpCodes.Dup);
                initializer.AcceptWalker(this, context);
                il_generator.Emit(OpCodes.Callvirt, add_method);
            }
            context.OperationTypeOnIdentifier = prev_op_type;

            return generic_type;
        }

        Type EmitListInitForDictionary(Type keyType, Type valueType, IEnumerable<KeyValueLikeExpression> initializers, CSharpEmitterContext context)
        {
            var dict_type = typeof(Dictionary<,>);
            var generic_type = dict_type.MakeGenericType(keyType, valueType);
            var add_method = generic_type.GetMethod("Add");
            var ctor = generic_type.GetConstructor(new Type[]{});

            var prev_op_type = context.OperationTypeOnIdentifier;
            context.OperationTypeOnIdentifier = OperationType.Load;
            il_generator.Emit(OpCodes.Newobj, ctor);
            foreach(var initializer in initializers){
                il_generator.Emit(OpCodes.Dup);
                initializer.AcceptWalker(this, context);
                il_generator.Emit(OpCodes.Callvirt, add_method);
            }
            context.OperationTypeOnIdentifier = prev_op_type;

            return generic_type;
        }

        void EmitSetElem(Type targetElementType)
        {
            if(!targetElementType.IsPrimitive)
                il_generator.Emit(OpCodes.Stelem_Ref);
            else if(targetElementType == typeof(byte) || targetElementType == typeof(sbyte))
                il_generator.Emit(OpCodes.Stelem_I1);
            else if(targetElementType == typeof(short) || targetElementType == typeof(ushort) || targetElementType == typeof(char))
                il_generator.Emit(OpCodes.Stelem_I2);
            else if(targetElementType == typeof(int) || targetElementType == typeof(uint))
                il_generator.Emit(OpCodes.Stelem_I4);
            else if(targetElementType == typeof(long) || targetElementType == typeof(ulong))
                il_generator.Emit(OpCodes.Stelem_I8);
            else if(targetElementType == typeof(float))
                il_generator.Emit(OpCodes.Stelem_R4);
            else if(targetElementType == typeof(double))
                il_generator.Emit(OpCodes.Stelem_R8);
            else
                il_generator.Emit(OpCodes.Stelem, targetElementType);
        }

        void EmitLoadElem(Type targetElementType)
        {
            if(!targetElementType.IsPrimitive)
                il_generator.Emit(OpCodes.Ldelem_Ref);
            else if(targetElementType == typeof(byte))
                il_generator.Emit(OpCodes.Ldelem_U1);
            else if(targetElementType == typeof(sbyte))
                il_generator.Emit(OpCodes.Ldelem_I1);
            else if(targetElementType == typeof(ushort))
                il_generator.Emit(OpCodes.Ldelem_U2);
            else if(targetElementType == typeof(short))
                il_generator.Emit(OpCodes.Ldelem_I2);
            else if(targetElementType == typeof(uint))
                il_generator.Emit(OpCodes.Ldelem_U4);
            else if(targetElementType == typeof(int))
                il_generator.Emit(OpCodes.Ldelem_I4);
            else if(targetElementType == typeof(long) || targetElementType == typeof(ulong))
                il_generator.Emit(OpCodes.Ldelem_I8);
            else if(targetElementType == typeof(float))
                il_generator.Emit(OpCodes.Ldelem_R4);
            else if(targetElementType == typeof(double))
                il_generator.Emit(OpCodes.Ldelem_R8);
            else
                il_generator.Emit(OpCodes.Ldelem, targetElementType);
        }
        #endregion
		#endregion
	}
}

