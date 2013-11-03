using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Expresso.Interpreter;
using Expresso.Runtime;
using Expresso.Compiler;
using Expresso.Compiler.Meta;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// スコープをもつ文の基底クラス。
	/// Represents a scope.
    /// </summary>
    public abstract class ScopeStatement : Statement/*, CompoundStatement*/
    {
		Dictionary<string, ExpressoVariable> variables;          // mapping of string to variables
		//ClosureInfo[] _closureVariables;                        // closed over variables, bool indicates if we accessed it in this scope.
		//List<PythonVariable> _freeVars;                         // list of variables accessed from outer scopes
		List<string> global_vars;                                // global variables accessed from this scope
		//List<string> _cellVars;                                 // variables accessed from nested scopes
		Dictionary<string, ExpressoReference> references;        // names of all variables referenced, null after binding completes

		internal Dictionary<string, ExpressoVariable> Variables{
			get{return variables;}
		}

		internal bool ContainsExceptionHandling{get; set;}
		
		internal virtual bool IsGlobal{
			get{return false;}
		}

		internal virtual string[] ParameterNames{
			get{
				return null;
			}
		}
		
		internal virtual int ArgCount{
			get{
				return 0;
			}
		}

		/// <summary>
		/// Variables that are bound to the global scope(the global module; usually it's the "main" module)
		/// </summary>
		internal IList<string> GlobalVariables{
			get{
				return global_vars;
			}
		}

		internal string AddReferencedGlobal(string name)
		{
			if(global_vars == null)
				global_vars = new List<string>();

			if(!global_vars.Contains(name))
				global_vars.Add(name);

			return name;
		}

		internal abstract bool ExposesLocalVariable(ExpressoVariable variable);
		
		bool TryGetAnyVariable(string name, out ExpressoVariable variable)
		{
			if(variables != null){
				return variables.TryGetValue(name, out variable);
			}else{
				variable = null;
				return false;
			}
		}
		
		internal bool TryGetVariable(string name, out ExpressoVariable variable)
		{
			if(TryGetAnyVariable(name, out variable)){
				return true;
			}else{
				variable = null;
				return false;
			}
		}
		
		internal virtual bool TryBindOuter(ScopeStatement from, ExpressoReference reference, out ExpressoVariable variable)
		{
			// Hide scope contents by default (only functions expose their locals)
			variable = null;
			return false;
		}
		
		internal abstract ExpressoVariable BindReference(ExpressoNameBinder binder, ExpressoReference reference);
		
		internal virtual void Bind(ExpressoNameBinder binder)
		{
			if(references != null){
				foreach(var reference in references.Values){
					//ExpressoVariable variable;
					reference.Variable /*= variable*/ = BindReference(binder, reference);
					
					// Accessing outer scope variable which is being deleted?
					/*if (variable != null) {
						if (variable.Deleted && variable.Scope != this && !variable.Scope.IsGlobal) {
							
							// report syntax error
							binder.ReportSyntaxError(
								String.Format(
								System.Globalization.CultureInfo.InvariantCulture,
								"can not delete variable '{0}' referenced in nested scope",
								reference.Name
								),
								this);
						}
					}*/
				}
			}
		}

		internal virtual void FinishBind(ExpressoNameBinder binder)
		{
			/*List<ClosureInfo> closureVariables = null;
			
			if (FreeVariables != null && FreeVariables.Count > 0) {
				_localParentTuple = Ast.Parameter(Parent.GetClosureTupleType(), "$tuple");
				
				foreach (var variable in _freeVars) {
					var parentClosure = Parent._closureVariables;                    
					Debug.Assert(parentClosure != null);
					
					for (int i = 0; i < parentClosure.Length; i++) {
						if (parentClosure[i].Variable == variable) {
							_variableMapping[variable] = new ClosureExpression(variable, Ast.Property(_localParentTuple, String.Format("Item{0:D3}", i)), null);
							break;
						}
					}
					Debug.Assert(_variableMapping.ContainsKey(variable));
					
					if (closureVariables == null) {
						closureVariables = new List<ClosureInfo>();
					}
					closureVariables.Add(new ClosureInfo(variable, !(this is ClassDefinition)));
				}
			}
			
			if (Variables != null) {
				foreach (PythonVariable variable in Variables.Values) {
					if (!HasClosureVariable(closureVariables, variable) &&
					    !variable.IsGlobal && (variable.AccessedInNestedScope || ExposesLocalVariable(variable))) {
						if (closureVariables == null) {
							closureVariables = new List<ClosureInfo>();
						}
						closureVariables.Add(new ClosureInfo(variable, true));
					}
					
					if (variable.Kind == VariableKind.Local) {
						Debug.Assert(variable.Scope == this);
						
						if (variable.AccessedInNestedScope || ExposesLocalVariable(variable)) {
							_variableMapping[variable] = new ClosureExpression(variable, Ast.Parameter(typeof(ClosureCell), variable.Name), null);
						} else {
							_variableMapping[variable] = Ast.Parameter(typeof(object), variable.Name);
						}
					}
				}
			}
			
			if (closureVariables != null) {
				_closureVariables = closureVariables.ToArray();
			}*/
			
			// no longer needed
			references = null;
		}

		void EnsureVariables()
		{
			if(variables == null)
				variables = new Dictionary<string, ExpressoVariable>(StringComparer.Ordinal);
		}
		
		internal void AddGlobalVariable(ExpressoVariable variable)
		{
			EnsureVariables();
			variables[variable.Name] = variable;
		}
		
		internal ExpressoReference Reference(string name)
		{
			if(references == null)
				references = new Dictionary<string, ExpressoReference>(StringComparer.Ordinal);

			ExpressoReference reference;
			if(!references.TryGetValue(name, out reference))
				references[name] = reference = new ExpressoReference(name, null);
			
			return reference;
		}
		
		internal bool IsReferenced(string name)
		{
			ExpressoReference reference;
			return references != null && references.TryGetValue(name, out reference);
		}
		
		internal ExpressoVariable CreateVariable(string name, TypeAnnotation type, VariableKind kind)
		{
			EnsureVariables();
			Debug.Assert(!variables.ContainsKey(name));
			ExpressoVariable variable;
			variables[name] = variable = new ExpressoVariable(name, type, kind, -1, this);
			return variable;
		}
		
		internal ExpressoVariable EnsureVariable(string name, TypeAnnotation type)
		{
			ExpressoVariable variable;
			if(!TryGetVariable(name, out variable))
				return CreateVariable(name, type, VariableKind.Local);
			
			return variable;
		}
		
		internal ExpressoVariable DefineParameter(string name, TypeAnnotation type)
		{
			return CreateVariable(name, type, VariableKind.Parameter);
		}
    }
}
