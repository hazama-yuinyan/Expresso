using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Expresso.Ast;
using Expresso.Ast.Analysis;
using Expresso.Runtime.Builtins;
using System.Collections;

namespace Expresso.CodeGen
{
    using CSharpExpr = System.Linq.Expressions.Expression;
    using ExprTree = System.Linq.Expressions;

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
    public class CSharpEmitter : IAstWalker<CSharpEmitterContext, CSharpExpr>
	{
        //###################################################
        //# Symbols defined in the whole program.
        //# It is represented as a hash table rather than a symbol table
        //# because we have defined a symbol id on all the identifier nodes
        //# that identifies the symbol uniqueness within the whole program.
        //###################################################
        static Dictionary<uint, ExpressoSymbol> Symbols = new Dictionary<uint, ExpressoSymbol>();
        static int LoopCounter = 1;
        static ExprTree.LabelTarget ReturnTarget = CSharpExpr.Label("returnTarget");
		
        bool has_continue;

        SymbolTable symbol_table;
        ExpressoCompilerOptions options;

        List<ExprTree.LabelTarget> break_targets = new List<ExprTree.LabelTarget>();
        List<ExprTree.LabelTarget> continue_targets = new List<ExprTree.LabelTarget>();

        int sibling_count = 0;

        /// <summary>
        /// Gets the generated assembly.
        /// </summary>
        public AssemblyBuilder AssemblyBuilder{
            get; private set;
        }

        static CSharpEmitter()
        {
            Symbols.Add(1000000000u, new ExpressoSymbol{
                Method = typeof(Console).GetMethod("Write", new []{
                    typeof(string), typeof(object[])
                })
            });
            Symbols.Add(1000000001u, new ExpressoSymbol{
                Method = typeof(Console).GetMethod("WriteLine", new []{
                    typeof(string), typeof(object[])
                })
            });
            Symbols.Add(1000000002u, new ExpressoSymbol{
                Method = typeof(List<>).GetMethod("Add")
            });
        }

        public CSharpEmitter(Parser parser, ExpressoCompilerOptions options)
        {
            symbol_table = parser.Symbols;
            this.options = options;
        }

        static Type GuessEnumeratorType(Type type)
        {
            var enumerator_type = type.GetNestedType("Enumerator");
            if(enumerator_type != null){
                foreach(var interface_type in enumerator_type.GetInterfaces()){
                    if(interface_type.FullName.StartsWith("System.Collections.Generic.IEnumerator")){
                        if(interface_type.ContainsGenericParameters)
                            interface_type.MakeGenericType(type.GetGenericArguments());
                        else
                            return interface_type;
                    }
                }

                throw new EmitterException("Type `{0}` has to implement IEnumerator<> interface", enumerator_type.FullName);
            }

            if(type.IsSubclassOf(typeof(IEnumerable)))
                return typeof(IEnumerator<>).MakeGenericType(type.GetGenericArguments());
            else
                throw new EmitterException("Type `{0}` has to implement IEnumerable<> interface", type.FullName);
        }

        static IEnumerable<CSharpExpr> MakeEnumeratorCreations(IEnumerable<CSharpExpr> iterators, out List<ExprTree.ParameterExpression> parameters)
        {
            var creations = new List<CSharpExpr>();
            parameters = new List<ExprTree.ParameterExpression>();
            int counter = 1;
            foreach(var iterator in iterators){
                var get_enumerator_method = iterator.Type.GetMethod("GetEnumerator");
                var param = CSharpExpr.Parameter(get_enumerator_method.ReturnType, "__iter" + counter.ToString());
                ++counter;

                parameters.Add(param);
                creations.Add(CSharpExpr.Assign(param, CSharpExpr.Call(iterator, get_enumerator_method)));
            }

            return creations;
        }

        static IEnumerable<CSharpExpr> MakeIterableAssignments(IEnumerable<ExprTree.ParameterExpression> variables, IEnumerable<ExprTree.ParameterExpression> enumerators,
            ExprTree.LabelTarget breakTarget)
        {
            var contents = new List<CSharpExpr>();
            foreach(var pair in variables.Zip(enumerators,
                (l, r) => new Tuple<ExprTree.ParameterExpression, CSharpExpr>(l, r))){
                var iterator_type = typeof(IEnumerator<>).MakeGenericType(pair.Item1.Type);
                var move_method = iterator_type.GetInterface("IEnumerator").GetMethod("MoveNext");
                var move_call = CSharpExpr.Call(pair.Item2, move_method);
                var check_failure = CSharpExpr.IfThen(CSharpExpr.IsFalse(move_call), CSharpExpr.Goto(breakTarget));
                contents.Add(check_failure);

                var current_property = iterator_type.GetProperty("Current");
                contents.Add(CSharpExpr.Assign(pair.Item1, CSharpExpr.Property(pair.Item2, current_property)));
            }

            return contents;
        }

        void DescendScope()
        {
            symbol_table = symbol_table.Children[sibling_count];
        }

        void AscendScope()
        {
            symbol_table = symbol_table.Parent;
        }

        void ProceedToNextSibling()
        {
            symbol_table = symbol_table.Parent.Children[sibling_count++];
        }

        static bool CanReturn(ReturnStatement returnStmt)
        {
            return returnStmt.Parent is BlockStatement && returnStmt.Parent.Parent is FunctionDeclaration && ((FunctionDeclaration)returnStmt.Parent.Parent).Name != "main";
        }

        ExpressoSymbol GetNativeSymbol(Identifier ident)
        {
            ExpressoSymbol symbol;
            if(Symbols.TryGetValue(ident.IdentifierId, out symbol))
                return symbol;
            else
                return null;
        }

        string GetModuleName(ExpressoAst ast)
        {
            return options.BuildType.HasFlag(BuildType.Assembly) ? ast.Name + ".dll" : ast.Name + ".exe";
        }

        string ConvertToCLRFunctionName(string name)
        {
            return name.Substring(0, 1).ToUpper() + name.Substring(1);
        }

        #region IAstWalker implementation

        public CSharpExpr VisitAst(ExpressoAst ast, CSharpEmitterContext context)
        {
            if(context == null)
                context = new CSharpEmitterContext();

            var name = new AssemblyName("exsAsm");

            var asm_builder = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave, options.OutputPath);
            var mod_builder = asm_builder.DefineDynamicModule(ast.Name + ".exe");
            var type_builder = mod_builder.DefineType("ExsMain", TypeAttributes.Class, typeof(object));

            context.AssemblyBuilder = asm_builder;
            context.ModuleBuilder = mod_builder;
            context.TypeBuilder = type_builder;

            foreach(var import in ast.Imports)
                import.AcceptWalker(this, context);

            foreach(var decl in ast.Declarations)
                decl.AcceptWalker(this, context);

            type_builder.CreateType();
            asm_builder.Save(GetModuleName(ast));

            AssemblyBuilder = asm_builder;

            return null;
        }

        public CSharpExpr VisitBlock(BlockStatement block, CSharpEmitterContext context)
        {
            int tmp_counter = sibling_count;
            DescendScope();
            sibling_count = 0;

            var parent_block = context.ContextAst;
            context.ContextAst = block;

            var contents = new List<CSharpExpr>();
            foreach(var stmt in block.Statements){
                var tmp = stmt.AcceptWalker(this, context);
                if(tmp == null){
                    contents.AddRange(context.Additionals.Cast<ExprTree.Expression>());
                    context.Additionals.Clear();
                }else{
                    contents.Add(tmp);
                }
            }

            context.ContextAst = parent_block;

            var variables = ConvertSymbolsToParameters().ToList();
            if(context.ContextAst is FunctionDeclaration && block.Statements.Last() is ReturnStatement){
                var return_expr = contents.Last() as ExprTree.GotoExpression;
                if(return_expr == null)
                    throw new EmitterException("Unknown state found! Expected the last expression is a GotoExpression!");

                contents.RemoveAt(contents.Count - 1);
                // Don't add an assignment expression here because visiting the return statement has done that
                //contents.Add(CSharpExpr.Assign(context.ParameterReturnValue, return_expr.Value));
                contents.Add(CSharpExpr.Label(ReturnTarget));
                contents.Add(context.ParameterReturnValue);
                variables.Add(context.ParameterReturnValue);
            }

            AscendScope();
            sibling_count = tmp_counter + 1;
            return CSharpExpr.Block(contents.Last().Type, variables, contents);
        }

        public CSharpExpr VisitBreakStatement(BreakStatement breakStmt, CSharpEmitterContext context)
        {
            int count = (int)breakStmt.Count.Value;
            if(count > break_targets.Count)
                throw new EmitterException("Can not break out of loops that many times!");

            //break upto Count; => goto label;
            return CSharpExpr.Break(break_targets[break_targets.Count - count]);
        }

        public CSharpExpr VisitContinueStatement(ContinueStatement continueStmt, CSharpEmitterContext context)
        {
            int count = (int)continueStmt.Count.Value;
            if(count > continue_targets.Count)
                throw new EmitterException("Can not break out of loops that many times!");

            //continue upto Count; => goto label;
            return CSharpExpr.Continue(continue_targets[continue_targets.Count - count]);
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
            int tmp_counter = sibling_count;
            DescendScope();
            sibling_count = 0;

            forStmt.Left.AcceptWalker(this, context);
            var iterator = forStmt.Target.AcceptWalker(this, context);
            var iterators = new List<CSharpExpr>{iterator};

            var break_target = CSharpExpr.Label("__EndFor" + LoopCounter.ToString());
            var continue_target = CSharpExpr.Label("__StartFor" + LoopCounter.ToString());
            ++LoopCounter;
            break_targets.Add(break_target);
            continue_targets.Add(continue_target);

            // Here, `Body` represents just the body block itself
            // In a for statement, we must move the iterator a step forward
            // and assign the result to inner-scope variables
            var real_body = forStmt.Body.AcceptWalker(this, context) as ExprTree.BlockExpression;

            List<ExprTree.ParameterExpression> enumerators;
            var variables = ConvertSymbolsToParameters();
            var creations = MakeEnumeratorCreations(iterators, out enumerators);
            var assignments = MakeIterableAssignments(variables, enumerators, break_target);

            var body = CSharpExpr.Block(variables,
                assignments.Concat(real_body.Expressions)
            );
            var loop = CSharpExpr.Loop(body, break_target, continue_target);
            break_targets.RemoveAt(break_targets.Count - 1);
            continue_targets.RemoveAt(continue_targets.Count - 1);
            AscendScope();
            sibling_count = tmp_counter + 1;

            var contents = new List<CSharpExpr>();
            contents.AddRange(creations);
            contents.Add(loop);
            return CSharpExpr.Block(enumerators, contents);
        }

        public CSharpExpr VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStatement, CSharpEmitterContext context)
        {
            int tmp_counter = sibling_count;
            DescendScope();
            sibling_count = 0;

            // TODO: Implement it in a more formal way(take multiple items into account)
            var bound_variables = new List<ExprTree.ParameterExpression>();
            var iterators = new List<CSharpExpr>();
            foreach(var variable in valueBindingForStatement.Variables){
                var bound_variable = CSharpExpr.Variable(CSharpCompilerHelper.GetNativeType(variable.NameToken.Type), variable.Name);
                Symbols.Add(variable.NameToken.IdentifierId, new ExpressoSymbol{Parameter = bound_variable});
                bound_variables.Add(bound_variable);
                iterators.Add(variable.Initializer.AcceptWalker(this, context));
            }

            var break_target = CSharpExpr.Label("__EndFor" + LoopCounter.ToString());
            var continue_target = CSharpExpr.Label("__StartFor" + LoopCounter.ToString());
            ++LoopCounter;
            break_targets.Add(break_target);
            continue_targets.Add(continue_target);

            // Here, `body` represents just the body block itself
            // In a for statement, we must move the iterator a step forward
            // and assign the result to inner-scope variables
            var real_body = valueBindingForStatement.Body.AcceptWalker(this, context) as ExprTree.BlockExpression;

            List<ExprTree.ParameterExpression> enumerators;
            var creations = MakeEnumeratorCreations(iterators, out enumerators);
            var assignments = MakeIterableAssignments(bound_variables, enumerators, break_target);
            var parameters = ConvertSymbolsToParameters();

            var body = CSharpExpr.Block(parameters,
                assignments.Concat(real_body.Expressions)
            );
            var loop = CSharpExpr.Loop(body, break_target, continue_target);
            break_targets.RemoveAt(break_targets.Count - 1);
            continue_targets.RemoveAt(continue_targets.Count - 1);
            AscendScope();
            sibling_count = tmp_counter + 1;

            var contents = new List<CSharpExpr>();
            contents.AddRange(creations);
            contents.Add(loop);
            return CSharpExpr.Block(enumerators, contents);
        }

        public CSharpExpr VisitIfStatement(IfStatement ifStmt, CSharpEmitterContext context)
        {
            int tmp_counter = sibling_count;
            DescendScope();
            sibling_count = 0;

            var cond = ifStmt.Condition.AcceptWalker(this, context);
            var true_block = ifStmt.TrueBlock.AcceptWalker(this, context);

            if(ifStmt.FalseBlock.IsNull){
                AscendScope();
                sibling_count = tmp_counter + 1;
                return CSharpExpr.IfThen(cond, true_block);
            }else{
                var false_block = ifStmt.FalseBlock.AcceptWalker(this, context);
                AscendScope();
                sibling_count = tmp_counter + 1;
                return CSharpExpr.IfThenElse(cond, true_block, false_block);
            }
        }

        public CSharpExpr VisitReturnStatement(ReturnStatement returnStmt, CSharpEmitterContext context)
        {
            // If we are in the main function, then make return statements do nothing
            if(!CanReturn(returnStmt))
                return CSharpExpr.Empty();

            var expr = returnStmt.Expression.AcceptWalker(this, context);
            var assign = CSharpExpr.Assign(context.ParameterReturnValue, expr);
            var return_expr = CSharpExpr.Return(ReturnTarget, expr);
            context.Additionals.Add(assign);
            context.Additionals.Add(return_expr);
            return null;
        }

        public CSharpExpr VisitMatchStatement(MatchStatement matchStmt, CSharpEmitterContext context)
        {
            // Match statement semantics: First we evaluate the target expression
            // and assign the result to a temporary variable that's alive within the whole statement.
            // All the pattern clauses must meet the same condition.
            // If context.ContextExpression is an ExprTree.ConditionalExpression
            // we know that we're at least at the second branch.
            // If it is null, then we're at the first branch so just set it the context expression.
            var target = matchStmt.Target.AcceptWalker(this, context);
            var target_var = CSharpExpr.Parameter(target.Type);
            context.TemporaryVariable = target_var;
            context.ContextExpression = null;
            var context_ast = context.ContextAst;
            context.ContextAst = matchStmt;

            int tmp_counter = sibling_count;
            DescendScope();
            sibling_count = 0;

            foreach(var clause in matchStmt.Clauses){
                var tmp = clause.AcceptWalker(this, context);
                if(context.ContextExpression != null){
                    var cond_expr = (ExprTree.ConditionalExpression)context.ContextExpression;
                    cond_expr.Update(cond_expr.Test, cond_expr.IfTrue, tmp);
                    context.ContextExpression = tmp;
                }else{
                    context.ContextExpression = tmp;
                }
            }

            AscendScope();
            sibling_count = tmp_counter + 1;

            context.TemporaryVariable = null;
            context.ContextAst = context_ast;

            return CSharpExpr.Block(new List<ExprTree.ParameterExpression>{
                target_var
            }, new List<CSharpExpr>{
                CSharpExpr.Assign(target_var, target),
                context.ContextExpression
            });
        }

        public CSharpExpr VisitWhileStatement(WhileStatement whileStmt, CSharpEmitterContext context)
        {
            has_continue = false;

            var end_loop = CSharpExpr.Label("__EndWhile" + LoopCounter.ToString());
            var continue_loop = CSharpExpr.Label("__BeginWhile" + LoopCounter.ToString());
            ++LoopCounter;
            break_targets.Add(end_loop);
            continue_targets.Add(continue_loop);

            int tmp_counter = sibling_count;
            DescendScope();
            sibling_count = 0;

            var condition = CSharpExpr.IfThen(CSharpExpr.Not(whileStmt.Condition.AcceptWalker(this, context)),
                CSharpExpr.Break(end_loop));
            var body = CSharpExpr.Block(
                condition,
                whileStmt.Body.AcceptWalker(this, context)
            );
            break_targets.RemoveAt(break_targets.Count - 1);
            continue_targets.RemoveAt(continue_targets.Count - 1);

            AscendScope();
            sibling_count = tmp_counter + 1;

            return has_continue ? CSharpExpr.Loop(body, end_loop, continue_loop) :
                CSharpExpr.Loop(body, end_loop);        //while(condition){body...}
        }

        public CSharpExpr VisitYieldStatement(YieldStatement yieldStmt, CSharpEmitterContext context)
        {
            throw new NotImplementedException();
        }

        public CSharpExpr VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl, CSharpEmitterContext context)
        {
            var decls = new List<CSharpExpr>();
            context.ContextAst = varDecl;
            foreach(var variable in varDecl.Variables){
                var tmp = variable.AcceptWalker(this, context);
                decls.Add(tmp);
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
            var compiled_args = new List<CSharpExpr>();
            foreach(var arg in call.Arguments)
                compiled_args.Add(arg.AcceptWalker(this, context));

            var inst = call.Target.AcceptWalker(this, context);
            return ConstructCallExpression((ExprTree.ParameterExpression)inst, context.Method, compiled_args);
        }

        public CSharpExpr VisitCastExpression(CastExpression castExpr, CSharpEmitterContext context)
        {
            var target = castExpr.Target.AcceptWalker(this, context);
            var to_type = CSharpCompilerHelper.GetNativeType(castExpr.ToExpression);
            return CSharpExpr.TypeAs(target, to_type);
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

        public CSharpExpr VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue, CSharpEmitterContext context)
        {
            var ident = keyValue.KeyExpression as PathExpression;
            if(ident != null){
                if(context.TargetType == null)
                    throw new EmitterException("Can not create an object of UNKNOWN!");

                var field = context.TargetType.GetField(ident.AsIdentifier.Name);
                if(field == null){
                    throw new EmitterException(
                        "Type `{0}` does not have the field `{1}`.",
                        context.TargetType, ident.AsIdentifier.Name
                    );
                }
                context.Field = field;

                var value = keyValue.ValueExpression.AcceptWalker(this, context);
                return value;
            }else{
                // In a dictionary literal, the key can be any expression that is evaluated
                // to a hashable object.
                var key_expr = keyValue.KeyExpression.AcceptWalker(this, context);
                var value_expr = keyValue.ValueExpression.AcceptWalker(this, context);
                if(context.TargetType == null)
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
                var native_type = CSharpCompilerHelper.GetNativeType(literal.Type);
                return CSharpExpr.Constant(literal.Value, native_type);
            }
        }

        public CSharpExpr VisitIdentifier(Identifier ident, CSharpEmitterContext context)
        {
            var symbol = GetNativeSymbol(ident);
            if(symbol != null){
                if(symbol.Parameter != null){
                    return symbol.Parameter;
                }else if(context.RequestType && symbol.Type != null){
                    context.TargetType = symbol.Type;
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
                if(context.RequestField){
                    var field = (context.TargetType != null) ? context.TargetType.GetField(ident.Name) : context.TypeBuilder.GetDeclaredField(ident.Name);
                    context.Field = field;
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
                intSeq.Lower.AcceptWalker(this, context),
                intSeq.Upper.AcceptWalker(this, context),
                intSeq.Step.AcceptWalker(this, context),
                CSharpExpr.Constant(intSeq.UpperInclusive)
            };
            return CSharpExpr.New(intseq_ctor, args);      //new ExpressoIntegerSequence(Start, End, Step, UpperInclusive)
        }

        public CSharpExpr VisitIndexerExpression(IndexerExpression indexExpr, CSharpEmitterContext context)
        {
            var target = indexExpr.Target.AcceptWalker(this, context);
            var args = new List<CSharpExpr>();
            foreach(var arg_expr in indexExpr.Arguments){
                var tmp = arg_expr.AcceptWalker(this, context);
                args.Add(tmp);
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
            // In Expresso, a member access can be resolved either to a field reference or instance method call
            var expr = memRef.Target.AcceptWalker(this, context);
            context.RequestField = true;
            context.RequestMethod = true;
            context.Field = null;
            context.Method = null;

            memRef.Member.AcceptWalker(this, context);
            context.RequestField = false;
            context.RequestMethod = false;
            return (context.Method != null) ? expr : CSharpExpr.Field(expr, context.Field);
        }

        public CSharpExpr VisitNewExpression(NewExpression newExpr, CSharpEmitterContext context)
        {
            var additionals = context.Additionals;
            context.Additionals = new List<object>();
            // On .NET environment, we have no means of creating object instances on the stack.
            newExpr.CreationExpression.AcceptWalker(this, context);
            var args = context.Additionals.Cast<ExprTree.Expression>();
            context.Additionals = additionals;
            return CSharpExpr.New(context.Constructor, args);
        }

        public CSharpExpr VisitPathExpression(PathExpression pathExpr, CSharpEmitterContext context)
        {
            if(pathExpr.AsIdentifier != null){
                context.RequestType = true;
                context.RequestMethod = true;
                context.TargetType = null;
                context.Method = null;

                var item = pathExpr.AsIdentifier.AcceptWalker(this, context);
                context.RequestType = false;
                context.RequestMethod = false;
                return item;
            }

            // On .NET environment, a path item is mapped to
            // Assembly::[Module]::{Class}
            // In reverse, an Expresso item can be mapped to the .NET type system as
            // Module.{Class}
            // Usually modules are converted to assemblies on themselves
            foreach(var ident in pathExpr.Items){
                if(context.Assembly == null){
                    Assembly asm = null;
                    foreach(var tmp in AppDomain.CurrentDomain.GetAssemblies()){
                        if(tmp.FullName == ident.Name){
                            asm = tmp;
                            break;
                        }
                    }
                    if(asm == null)
                        throw new EmitterException("Assembly `{0}` not found!", ident.Name);

                    context.Assembly = asm;
                }else if(context.Module == null){
                    var module = context.Assembly.GetModule(ident.Name);
                    if(module != null)
                        context.Module = module;
                    else
                        throw new EmitterException("Module `{0}` isn't defined in Assembly `{1}`", ident.Name, context.Assembly.FullName);
                }else if(context.TargetType == null){
                    var type = context.Assembly.GetType(ident.Name);
                    if(type != null){
                        context.TargetType = type;
                        if(context.Constructor == null)
                            context.Constructor = type.GetConstructors().Last();
                    }else{
                        throw new EmitterException("Type `{0}` isn't defined in Assembly `{1}`", ident.Name, context.Assembly.FullName);
                    }
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
            }

            return null;
        }

        public CSharpExpr VisitParenthesizedExpression(ParenthesizedExpression parensExpr, CSharpEmitterContext context)
        {
            return parensExpr.Expression.AcceptWalker(this, context);
        }

        public CSharpExpr VisitObjectCreationExpression(ObjectCreationExpression creation, CSharpEmitterContext context)
        {
            var args = new List<CSharpExpr>(creation.Items.Count);
            context.Constructor = null;
            creation.TypePath.AcceptWalker(this, context);
            if(context.Constructor == null)
                throw new EmitterException("No constructor found for the path `{0}`", creation.TypePath);

            var formal_params = context.Constructor.GetParameters();
            foreach(var pair in creation.Items){
                context.Field = null;
                var value_expr = pair.AcceptWalker(this, context);

                int index = 0;
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
                }
                args[index] = value_expr;
            }

            return CSharpExpr.New(context.Constructor, args);
        }

        public CSharpExpr VisitSequenceInitializer(SequenceInitializer seqInitializer, CSharpEmitterContext context)
        {
            var obj_type = seqInitializer.ObjectType;
            var seq_type = CSharpCompilerHelper.GetContainerType(obj_type);
            var exprs = new List<CSharpExpr>();
            // If this node represents a dictionary literal
            // context.Constructor will get set the appropriate constructor method.
            context.Constructor = null;
            var additionals = context.Additionals;
            context.Additionals = new List<object>();
            foreach(var item in seqInitializer.Items){
                var tmp = item.AcceptWalker(this, context);
                exprs.Add(tmp);
            }

            if(seq_type == typeof(Array)){
                var elem_type = CSharpCompilerHelper.GetNativeType(obj_type.TypeArguments.FirstOrNullObject());
                context.Additionals = additionals;
                return CSharpExpr.NewArrayInit(elem_type, exprs);
            }else if(seq_type == typeof(List<>)){
                var elem_type = CSharpCompilerHelper.GetNativeType(obj_type.TypeArguments.FirstOrNullObject());
                var list_type = seq_type.MakeGenericType(elem_type);
                var constructor = list_type.GetConstructor(new Type[]{});
                var new_expr = CSharpExpr.New(constructor);
                context.Additionals = additionals;
                return (exprs.Count == 0) ? new_expr : (CSharpExpr)CSharpExpr.ListInit(new_expr, exprs);
            }else if(seq_type == typeof(Dictionary<,>)){
                var key_type = CSharpCompilerHelper.GetNativeType(obj_type.TypeArguments.FirstOrNullObject());
                var value_type = CSharpCompilerHelper.GetNativeType(obj_type.TypeArguments.LastOrNullObject());
                var elems = context.Additionals.Cast<ExprTree.ElementInit>();
                var dict_type = seq_type.MakeGenericType(key_type, value_type);
                var constructor = dict_type.GetConstructor(new Type[]{});
                var new_expr = CSharpExpr.New(constructor);
                context.Additionals = additionals;
                return (exprs.Count == 0) ? new_expr : (CSharpExpr)CSharpExpr.ListInit(new_expr, elems);
            }else if(seq_type == typeof(Tuple)){
                var child_types = 
                    from e in exprs
                    select e.Type;
                var ctor_method = typeof(Tuple).GetMethod("Create", child_types.ToArray());
                context.Additionals = additionals;
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
            //           Test{1, _} => print("1, x");
            //           Test{2, 2} => print("2, 2");
            //           Test{3, _} => print("3, x");
            //           Test{x, y} if y == 2 * x => print("y == 2 * x");
            //           Test{x, _} => print("{}, y", x);
            //       }
            // =>    if(t is Test && t.x == 1){
            //           Console.Write("1, x");
            //       }else if(t is Test && t.x == 2 && t.y == 2){
            //           Console.Write("2, 2");
            //       }else if(t is Test && t.x == 3){
            //           Console.Write("3, x");
            //       }else if(t is Test && t.y == 2 * t.x){ //a guard becomes and-ed condition
            //           int x = t.x, y = t.y;    //destructuring becomes inner scope variable declarations
            //           Console.Write("y == 2 * x");
            //       }else if(t is Test){
            //           int x = t.x;
            //           Console.Write("{0}, y", x);
            //       }else{
            //           Console.Write("else");
            //       }
            IEnumerable<ExprTree.ParameterExpression> destructuring_exprs = null;
            CSharpExpr res = null;
            var prev_additionals = context.Additionals;
            foreach(var pattern in matchClause.Patterns){
                context.Additionals = null;

                var pattern_cond = pattern.AcceptWalker(this, context);
                if(context.Additionals != null){
                    // If the pattern contains destructuring
                    // then the first expression is the only source for the condition it explains.
                    if(destructuring_exprs == null){
                        destructuring_exprs = context.Additionals.Cast<ExprTree.ParameterExpression>();
                    }else if(destructuring_exprs.Count() != context.Additionals.Count()){
                        // The number of destructured variables must match in every pattern
                        throw new EmitterException(
                            "Expected the pattern contains {0} variables, but it only contains {1}!",
                            destructuring_exprs.Count(), context.Additionals.Count()
                        );
                    }
                }

                if(res == null)
                    res = pattern_cond;
                else
                    res = CSharpExpr.OrElse(res, pattern_cond);
            }

            context.Additionals = prev_additionals;
            var guard = matchClause.Guard.AcceptWalker(this, context);
            if(guard != null)
                res = CSharpExpr.AndAlso(res, guard);

            var body = matchClause.Body.AcceptWalker(this, context);
            if(destructuring_exprs != null)
                body = CSharpExpr.Block(destructuring_exprs, body);

            return (res == null) ? null : CSharpExpr.IfThen(res, body);
        }

        public CSharpExpr VisitSequence(SequenceExpression seqExpr, CSharpEmitterContext context)
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
            var cur_context_type = context.TypeBuilder.AsType();
            return CSharpExpr.Parameter(cur_context_type, "self");
        }

        public CSharpExpr VisitSuperReferenceExpression(SuperReferenceExpression superRef, CSharpEmitterContext context)
        {
            var super_type = context.TypeBuilder.BaseType;
            return CSharpExpr.Parameter(super_type, "super");
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
            return null;
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

            Symbols.Add(aliasDecl.AliasToken.IdentifierId, new ExpressoSymbol{
                Type = context.TargetType,
                Method = context.Method
            });
            return null;
        }

        public CSharpExpr VisitImportDeclaration(ImportDeclaration importDecl, CSharpEmitterContext context)
        {
            if(importDecl.ImportedEntities.IsEmpty){
                context.RequestType = true;
                context.RequestMethod = true;
                context.RequestField = true;
                context.TargetType = null;
                context.Method = null;
                context.Field = null;

                importDecl.ModuleNameToken.AcceptWalker(this, context);
                if(importDecl.AliasName != null){
                    var type_symbol = symbol_table.GetTypeSymbol(importDecl.AliasName);
                    if(type_symbol != null){
                        Symbols.Add(type_symbol.IdentifierId, new ExpressoSymbol{
                            Type = context.TargetType,
                            Field = context.Field,
                            Method = context.Method
                        });
                    }else{
                        var symbol = symbol_table.GetSymbol(importDecl.ModuleName);
                        Symbols.Add(symbol.IdentifierId, new ExpressoSymbol{
                            Type = context.TargetType,
                            Method = context.Method,
                            Field = context.Field
                        });
                    }
                }
            }else{
                foreach(var entity in importDecl.ImportedEntities){
                    context.RequestType = true;
                    context.RequestMethod = true;
                    context.RequestField = true;
                    context.TargetType = null;
                    context.Method = null;
                    context.Field = null;

                    // Walking through the path items registers the symbols in question
                    entity.AcceptWalker(this, context);
                }
            }

            return null;
        }

        public CSharpExpr VisitFunctionDeclaration(FunctionDeclaration funcDecl, CSharpEmitterContext context)
        {
            context.Additionals = new List<object>();
            int tmp_counter = sibling_count;
            DescendScope();
            sibling_count = 0;

            var context_ast = context.ContextAst;
            context.ContextAst = funcDecl;

            var param_types = new List<Type>();
            var parameters = new List<ExprTree.ParameterExpression>();
            foreach(var param in funcDecl.Parameters){
                var tmp = param.AcceptWalker(this, context);
                param_types.Add(tmp.Type);
                parameters.Add((ExprTree.ParameterExpression)tmp);
            }

            var prev_rv = context.ParameterReturnValue;
            var return_type = CSharpCompilerHelper.GetNativeType(funcDecl.ReturnType);
            if(return_type != typeof(void))
                context.ParameterReturnValue = CSharpExpr.Parameter(return_type, "return_value");

            var body = funcDecl.Body.AcceptWalker(this, context);
            context.Additionals = null;
            context.ContextAst = context_ast;
            context.ParameterReturnValue = prev_rv;
            var lambda = CSharpExpr.Lambda(body, parameters);

            var attr = MethodAttributes.Static | MethodAttributes.Family;
            if(funcDecl.Modifiers.HasFlag(Modifiers.Export)){
                attr |= MethodAttributes.Public;
                attr ^= MethodAttributes.Family;
            }

            if(funcDecl.Name == "main")
                attr |= MethodAttributes.HideBySig;

            var func_builder = context.TypeBuilder.DefineMethod(ConvertToCLRFunctionName(funcDecl.Name), attr, return_type, param_types.ToArray());
            lambda.CompileToMethod(func_builder);

            Symbols.Add(funcDecl.NameToken.IdentifierId, new ExpressoSymbol{Method = func_builder});
            if(funcDecl.Name == "main")
                context.AssemblyBuilder.SetEntryPoint(func_builder, PEFileKinds.ConsoleApplication);

            AscendScope();
            sibling_count = tmp_counter + 1;

            return null;
        }

        public CSharpExpr VisitTypeDeclaration(TypeDeclaration typeDecl, CSharpEmitterContext context)
        {
            var parent_type = context.TypeBuilder;
            var attr = typeDecl.Modifiers.HasFlag(Modifiers.Export) ? TypeAttributes.Public : TypeAttributes.NotPublic;
            attr |= TypeAttributes.Class;
            context.TypeBuilder = (parent_type != null) ? parent_type.DefineNestedType(typeDecl.Name, attr) : context.ModuleBuilder.DefineType(typeDecl.Name, attr);

            try{
                foreach(var member in typeDecl.Members)
                    member.AcceptWalker(this, context);

                var type = context.TypeBuilder.CreateType();
                Symbols.Add(typeDecl.NameToken.IdentifierId, new ExpressoSymbol{Type = type});
            }
            finally{
                context.TypeBuilder = parent_type;
            }
            return null;
        }

        public CSharpExpr VisitFieldDeclaration(FieldDeclaration fieldDecl, CSharpEmitterContext context)
        {
            FieldAttributes attr = FieldAttributes.Private;
            if(fieldDecl.Modifiers.HasFlag(Modifiers.Static))
                attr |= FieldAttributes.Static;

            if(fieldDecl.Modifiers.HasFlag(Modifiers.Immutable))
                attr |= FieldAttributes.InitOnly;

            if(fieldDecl.Modifiers.HasFlag(Modifiers.Private))
                attr |= FieldAttributes.Private;
            else if(fieldDecl.Modifiers.HasFlag(Modifiers.Protected))
                attr |= FieldAttributes.Family;
            else if(fieldDecl.Modifiers.HasFlag(Modifiers.Public))
                attr |= FieldAttributes.Public;
            else
                throw new EmitterException("Unknown modifiers!");

            foreach(var init in fieldDecl.Initializers){
                var type = CSharpCompilerHelper.GetNativeType(init.NameToken.Type);
                var field_builder = context.TypeBuilder.DefineField(init.Name, type, attr);
                var initializer = init.Initializer.AcceptWalker(this, context);
                var constant = initializer as ExprTree.ConstantExpression;
                if(constant != null)
                    field_builder.SetConstant(constant.Value);

                Symbols.Add(init.NameToken.IdentifierId, new ExpressoSymbol{Field = context.TypeBuilder.GetDeclaredField(init.Name)});
            }

            return null;
        }

        public CSharpExpr VisitParameterDeclaration(ParameterDeclaration parameterDecl, CSharpEmitterContext context)
        {
            var native_type = CSharpCompilerHelper.GetNativeType(parameterDecl.ReturnType);
            var param = CSharpExpr.Parameter(native_type, parameterDecl.Name);
            Symbols.Add(parameterDecl.NameToken.IdentifierId, new ExpressoSymbol{Parameter = param});
            if(context.Additionals != null)
                context.Additionals.Add(param);

            return param;
        }

        public CSharpExpr VisitVariableInitializer(VariableInitializer initializer, CSharpEmitterContext context)
        {
            var variable = CSharpExpr.Variable(CSharpCompilerHelper.GetNativeType(initializer.NameToken.Type), initializer.Name);
            var init = initializer.Initializer.AcceptWalker(this, context);
            if(context.ContextAst is VariableDeclarationStatement)
                Symbols.Add(initializer.NameToken.IdentifierId, new ExpressoSymbol{Parameter = variable});

            var result = (init == null) ? variable as CSharpExpr : CSharpExpr.Assign(variable, init);
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
            var ident = identifierPattern.Identifier.AcceptWalker(this, context);
            if(context.Additionals != null)
                context.Additionals.Add(ident);

            if(context.Field == null){
                var field = context.TargetType.GetField(identifierPattern.Identifier.Name);
                if(field == null){
                    throw new EmitterException(
                        "Type `{0}` doesn't have the field `{1}`",
                        context.TargetType, identifierPattern.Identifier.Name
                    );
                }
                context.Field = field;
            }

            return ident;
        }

        public CSharpExpr VisitValueBindingPattern(ValueBindingPattern valueBindingPattern, CSharpEmitterContext context)
        {
            // ValueBindingPatterns can be complex because they introduce new variables into the surrounding scope
            // and they have nothing to do with the value being matched.
            context.Additionals = new List<object>();
            var pattern = valueBindingPattern.Variables.Select(variable => variable.AcceptWalker(this, context));
            var parameters = context.Additionals.Cast<ExprTree.ParameterExpression>();
            context.ContextExpression = CSharpExpr.Block(parameters, context.ContextExpression);
            var result = CSharpExpr.Block(parameters, pattern);

            return result;
        }

        public CSharpExpr VisitCollectionPattern(CollectionPattern collectionPattern, CSharpEmitterContext context)
        {
            // First, make type validation expression
            var collection_type = CSharpCompilerHelper.GetContainerType(collectionPattern.CollectionType);
            CSharpExpr res = null;
            int i = 0;
            var block = new List<CSharpExpr>();
            foreach(var pattern in collectionPattern.Items){
                var tmp = pattern.AcceptWalker(this, context);
                var index = CSharpExpr.Constant(i++);
                var elem_access = CSharpExpr.ArrayIndex(context.TemporaryVariable, index);
                var param = tmp as ExprTree.ParameterExpression;
                if(param != null){
                    var assignment = CSharpExpr.Assign(param, elem_access);
                    block.Add(assignment);
                }

                if(res == null)
                    res = tmp;
                else
                    res = CSharpExpr.AndAlso(res, elem_access);
            }

            context.ContextExpression = CSharpExpr.Block(block);
            return CSharpExpr.TypeIs(context.TemporaryVariable, collection_type);
        }

        public CSharpExpr VisitDestructuringPattern(DestructuringPattern destructuringPattern, CSharpEmitterContext context)
        {
            context.TargetType = null;
            destructuringPattern.TypePath.AcceptWalker(this, context);
            var type = context.TargetType;

            var parameters = new List<CSharpExpr>();
            foreach(var pattern in destructuringPattern.Items){
                context.Field = null;
                var param = pattern.AcceptWalker(this, context) as ExprTree.ParameterExpression;
                parameters.Add(CSharpExpr.Assign(param, CSharpExpr.Field(context.TemporaryVariable, context.Field)));
            }

            context.ContextExpression = CSharpExpr.Block(parameters);
            return CSharpExpr.TypeIs(context.TemporaryVariable, type);
        }

        public CSharpExpr VisitTuplePattern(TuplePattern tuplePattern, CSharpEmitterContext context)
        {
            // Tuple patterns should always be combined with value binding patterns
            var element_types = new List<Type>();
            var exprs = new List<CSharpExpr>();
            int i = 1;
            foreach(var pattern in tuplePattern.Patterns){
                var tmp = pattern.AcceptWalker(this, context);
                var param = tmp as ExprTree.ParameterExpression;
                if(param != null){
                    var elem_name = "Item" + i.ToString();
                    exprs.Add(CSharpExpr.Assign(param, CSharpExpr.PropertyOrField(context.TemporaryVariable, elem_name)));
                }
                element_types.Add(tmp.Type);
                ++i;
            }

            var tuple_type = CSharpCompilerHelper.GuessTupleType(element_types);
            context.ContextExpression = CSharpExpr.Block(exprs);
            return CSharpExpr.TypeIs(context.TemporaryVariable, tuple_type);
        }

        public CSharpExpr VisitExpressionPattern(ExpressionPattern exprPattern, CSharpEmitterContext context)
        {
            // Common scinario in an expression pattern:
            // An integer sequence expression or a literal expression.
            // In the former case we should test an integer against an IntSeq type object using an IntSeq's method
            // while in the latter case we should just test the value against the literal
            context.Method = null;
            var expr = exprPattern.Expression.AcceptWalker(this, context);
            return (context.Method != null) ? CSharpExpr.Call(context.Method, expr) as CSharpExpr :
                (context.ContextAst is MatchStatement) ? CSharpExpr.Equal(context.TemporaryVariable, expr) as CSharpExpr : expr;
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
                return null;    // The parameter modifier "ref" is the primary candidate for this operator

            case OperatorType.Dereference:
                return null;    // There is no alternatives to dereferencing in C#

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

        CSharpExpr ConstructCallExpression(ExprTree.ParameterExpression inst, MethodInfo method, IEnumerable<CSharpExpr> args)
        {
            if(method.Name == "Write" || method.Name == "WriteLine"){
                var first = args.First();
                if(first.Type == typeof(string)){
                    method = typeof(Console).GetMethod(method.Name, new []{typeof(string), typeof(object[])});

                    return CSharpExpr.Call(method, first, CSharpExpr.NewArrayInit(typeof(object),
                        args.Skip(1).Select(a => CSharpExpr.Convert(a, typeof(object)))
                    ));
                }else{
                    var builder = new StringBuilder();
                    for(int i = 0; i < args.Count(); ++i){
                        if(i != 0)
                            builder.Append(", ");
                        
                        builder.Append("{" + i.ToString() + "}");
                    }

                    return CSharpExpr.Call(method, CSharpExpr.Constant(builder.ToString()), CSharpExpr.NewArrayInit(typeof(object),
                        args.Select(a => CSharpExpr.Convert(a, typeof(object)))
                    ));
                }
            }else{
                if(!(method is MethodBuilder) && method.ContainsGenericParameters){
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
                return (inst != null) ? CSharpExpr.Call(inst, method, args) : CSharpExpr.Call(method, args);
            }
        }
		#endregion
	}
}

