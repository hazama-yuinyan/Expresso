using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Expresso.CodeGen;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast.Analysis
{
    /// <summary>
    /// Represents the symbol table. The symbol table is represented by a n-branched tree
    /// with each node representing one scope(one SymbolTable instance).
    /// In Expresso, there are 2 kinds of namespaces.
    /// One is for types, and the other is for local variables, parameters, function names and method names.
    /// </summary>
    public class SymbolTable : ISerializable, ICloneable<SymbolTable>
    {
        static readonly Dictionary<string, Identifier> NativeMapping;
        static readonly string TypeTablePrefix = "type_";
        Dictionary<string, Identifier> type_table;
        MultiValueDictionary<string, Identifier> table;

        /// <summary>
        /// The name for this symbol scope.
        /// </summary>
        public string Name{
            get; set;
        }

        /// <summary>
        /// The parent symbol scope.
        /// </summary>
        public SymbolTable Parent{
            get; set;
        }

        /// <summary>
        /// The child symbol scopes.
        /// Even though it is defined as an array, it acts most of the times as if it were defined as
        /// bi-directional linked list.
        /// </summary>
        public List<SymbolTable> Children{
            get; set;
        }

        /// <summary>
        /// Gets all the symbols as an enumerator.
        /// </summary>
        public IEnumerable<Identifier> Symbols{
            get{
                return table.Values.SelectMany(col => col.Select(ident => ident));
            }
        }

        /// <summary>
        /// Gets the number of symbols within this scope.
        /// </summary>
        public int NumberOfSymbols{
            get{return Symbols.Count();}
        }

        /// <summary>
        /// Represents the kind of the type.
        /// </summary>
        /// <value>The kind of the type.</value>
        public ClassType TypeKind{
            get;
        }

        /// <summary>
        /// Gets the value indicating whether the symbol table represents a type in .NET.
        /// </summary>
        /// <value><c>true</c> if is net type; otherwise, <c>false</c>.</value>
        public bool IsNetType{
            get;
        }

        static SymbolTable()
        {
            NativeMapping = new Dictionary<string, Identifier>();
            var print_ident = AstNode.MakeIdentifier(
                "Write",
                AstType.MakeFunctionType(
                    "print",
                    AstType.MakeSimpleType("tuple", TextLocation.Empty),    //The void type
                    TextLocation.Empty,
                    TextLocation.Empty,
                    AstType.MakeSimpleType(
                        "array",
                        TextLocation.Empty,
                        TextLocation.Empty,
                        AstType.MakeSimpleType("object")
                    )
                )
            );
            print_ident.IdentifierId = 1000000000u;
            NativeMapping.Add("print", print_ident);

            var println_ident = AstNode.MakeIdentifier(
                "WriteLine",
                AstType.MakeFunctionType(
                    "println",
                    AstType.MakeSimpleType("tuple", TextLocation.Empty),    //The void type
                    TextLocation.Empty,
                    TextLocation.Empty,
                    AstType.MakeSimpleType(
                        "array",
                        TextLocation.Empty,
                        TextLocation.Empty,
                        AstType.MakeSimpleType("object")
                    )
                )
            );
            println_ident.IdentifierId = 1000000001u;
            NativeMapping.Add("println", println_ident);

            /*var printformat_ident = AstNode.MakeIdentifier(
                "WriteLine",
                AstType.MakeFunctionType(
                    "printFormat",
                    AstType.MakeSimpleType("tuple", TextLocation.Empty),    // The void type
                    TextLocation.Empty,
                    TextLocation.Empty,
                    AstType.MakePrimitiveType("string", TextLocation.Empty),
                    AstType.MakeSimpleType(
                        "array",
                        TextLocation.Empty,
                        TextLocation.Empty,
                        AstType.MakeSimpleType("object")
                    )
                )
            );
            printformat_ident.IdentifierId = 1000000002u;
            NativeMapping.Add("printFormat", printformat_ident);*/
        }

        public SymbolTable(ClassType typeKind = ClassType.NotType, bool isNetType = false)
        {
            type_table = new Dictionary<string, Identifier>();
            table = new MultiValueDictionary<string, Identifier>();
            Children = new List<SymbolTable>();
            TypeKind = typeKind;
            IsNetType = isNetType;
        }

        /// <summary>
        /// Creates a new instance of the SymbolTable class.
        /// </summary>
        /// <returns>The create.</returns>
        public static SymbolTable Create()
        {
            var table = new SymbolTable();
            table.Name = "programRoot";
            var table2 = new SymbolTable();
            table2.Name = "root";
            table.Parent = table2;
            table2.Children.Add(table);

            ExpressoCompilerHelpers.AddPrimitiveTypesSymbolTables(table2);
            return table;
        }

        public static Identifier GetNativeSymbol(string name)
        {
            if(NativeMapping.TryGetValue(name, out var result))
                return result;
            else
                return null;
        }

        #region ISerializable implementation

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach(var entry in type_table){
                var ast_type = entry.Value.Type;
                info.AddValue(entry.Key, ast_type, ast_type.GetType());
            }

            foreach(var entry2 in table){
                foreach(var list_value in entry2.Value){
                    var ast_type = list_value.Type;
                    info.AddValue(entry2.Key, ast_type, ast_type.GetType());
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets the type table named `name` in any parent scopes.
        /// </summary>
        /// <returns>The type table.</returns>
        /// <param name="name">Name.</param>
        public SymbolTable GetTypeTable(string name)
        {
            if(name == null)
                throw new ArgumentNullException();
            
            var parent = this;
            while(parent != null && parent.Children.Count == 0)
                parent = parent.Parent;

            if(parent == null)
                return null;
            
            var tmp = parent.Children[0];
            var class_name = TypeTablePrefix + name;
            var regex = new Regex($@"{class_name.Replace(".", "\\.")}(?![:\.\w]+)");
            int child_counter = 1;
            while(tmp != null){
                if(regex.IsMatch(tmp.Name))
                    return tmp;

                if(child_counter >= parent.Children.Count){
                    child_counter = 1;
                    do{
                        parent = parent.Parent;
                    }while(parent != null && parent.Children.Count == 0);
                    if(parent == null)
                        break;
                    
                    tmp = parent.Children[0];
                }else{
                    tmp = parent.Children[child_counter++];
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether `name` is a type symbol.
        /// </summary>
        /// <returns><c>true</c> if `name` is a type name; otherwise, <c>false</c>.</returns>
        /// <param name="name">The name to test.</param>
        public bool HasTypeSymbol(string name)
        {
            return type_table.ContainsKey(name);
        }

        public bool HasTypeSymbolInAnyScope(string name)
        {
            if(type_table.ContainsKey(name))
                return true;
            else if(Parent == null)
                return false;
            else
                return Parent.HasTypeSymbolInAnyScope(name);
        }

        /// <summary>
        /// Determines whether `name` is a symbol name(variables or functions).
        /// </summary>
        /// <returns><c>true</c> if `name` is a symbol name; otherwise, <c>false</c>.</returns>
        /// <param name="name">The name to test.</param>
        public bool HasSymbol(string name)
        {
            return table.ContainsKey(name);
        }

        public bool HasSymbolInAnyScope(string name)
        {
            if(table.ContainsKey(name))
                return true;
            else if(Parent == null)
                return false;
            else
                return Parent.HasSymbolInAnyScope(name);
        }

        /// <summary>
        /// Determines whether `name` represents any symbol name in the current and all the parent scopes.
        /// </summary>
        /// <returns><c>true</c> if `name` is declared as a symbol name; otherwise, <c>false</c>.</returns>
        /// <param name="name">The name to test.</param>
        public bool IsSymbol(string name)
        {
            if(HasTypeSymbol(name) || HasSymbol(name))
                return true;
            else
                return Parent.IsSymbol(name);
        }

        /// <summary>
        /// Adds a new name to the type namespace.
        /// Note that you must also specify the corresponding type that the name represents.
        /// At the first glance, `type` is unnecessary because type names almost always represent themselves
        /// but consider the situations where alias names are declared.
        /// The symbol table is responsible for managing ALL the names declared in scopes,
        /// so we must know which name represents which type in a certain scope.
        /// </summary>
        /// <param name="name">The symbol name to be added. It can be an alias name.</param>
        /// <param name="type">The type that corresponds to the name in the current or child scopes.</param>
        public void AddTypeSymbol(string name, AstType type)
        {
            AddTypeSymbol(name, AstNode.MakeIdentifier(name, type));
        }

        /// <summary>
        /// Adds a new name to the type namespace.
        /// Use this overload when encountered with a new type declaration.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="ident">Ident.</param>
        public void AddTypeSymbol(string name, Identifier ident)
        {
            try{
                type_table.Add(name, ident);
            }
            catch(ArgumentException){
                throw new ParserException("Error ES3014: The name `{0}` is already defined in the type namespace in the current scope '{1}'.", ident, name, Name);
            }
        }

        /// <summary>
        /// Adds a new name to the symbol scope.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="type">Type.</param>
        public void AddSymbol(string name, AstType type)
        {
            AddSymbol(name, AstNode.MakeIdentifier(name, type));
        }

        /// <summary>
        /// Adds a new name with a certain identifier.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="ident">Ident.</param>
        public void AddSymbol(string name, Identifier ident)
        {
            try{
                table.Add(name, ident);
            }
            catch(ArgumentException){
                throw new ParserException("Error ES3014: The name `{0}` is already defined in the current scope {1}.", ident, name, Name);
            }
        }

        /// <summary>
        /// Gets the corresponding <see cref="Expresso.Ast.Identifier"/> node to type `name`.
        /// </summary>
        public Identifier GetTypeSymbol(string name)
        {
            if(!type_table.TryGetValue(name, out var result))
                return null;

            return result;
        }

        public Identifier GetTypeSymbolInAnyScope(string name)
        {
            if(!type_table.TryGetValue(name, out var result)){
                if(Parent != null)
                    return Parent.GetTypeSymbolInAnyScope(name);
                else
                    return null;
            }

            return result;
        }

        /// <summary>
        /// Gets the corresponding <see cref="Expresso.Ast.Identifier"/> node to `name`.
        /// </summary>
        public Identifier GetSymbol(string name)
        {
            if(!table.TryGetValue(name, out var results))
                return null;

            if(results.Count > 1)
                throw new InvalidOperationException("There are more than 1 candidate for '" + name + "'");
            
            return results.FirstOrDefault();
        }

        /// <summary>
        /// Gets the corresponding <see cref="Identifier"/> node to `name` that also matches to the `type`.
        /// </summary>
        /// <returns>The symbol.</returns>
        /// <param name="name">Name.</param>
        /// <param name="type">Type.</param>
        public Identifier GetSymbol(string name, AstType type)
        {
            if(!table.TryGetValue(name, out var results))
                return null;

            return results.Where(ident => ident.Type.IsMatch(type))
                          .Select(ident => ident)
                          .FirstOrDefault();
        }

        /// <summary>
        /// Gets all the symbols corresponding to `name`.
        /// </summary>
        /// <returns>The symbols.</returns>
        /// <param name="name">Name.</param>
        public IReadOnlyCollection<Identifier> GetSymbols(string name)
        {
            if(!table.TryGetValue(name, out var results))
                return null;
            else
                return results;
        }

        /// <summary>
        /// Gets the corresponding <see cref="Expresso.Ast.Identifier"/> node to `name`, trying to track up, at most, n scopes.
        /// </summary>
        /// <returns>The symbol</returns>
        /// <param name="name">The symbol name to be fetched.</param>
        /// <param name="limit">The maximum count to track up scopes, while trying to find the symbol.</param>
        public Identifier GetSymbolInNScopesAbove(string name, int limit)
        {
            if(!table.TryGetValue(name, out var results)){
                if(Parent != null && limit > 0){
                    return Parent.GetSymbolInNScopesAbove(name, limit - 1);
                }else{
                    if(limit > 0 && NativeMapping.TryGetValue(name, out var result))
                        return result;
                    else
                        return null;
                }
            }

            if(results.Count > 1)
                throw new InvalidOperationException("There are more than 1 candidate for '" + name + "'");

            return results.FirstOrDefault();
        }

        /// <summary>
        /// Gets the corresponding <see cref="Identifier"/> node to `name` in any parent scopes.
        /// </summary>
        public Identifier GetSymbolInAnyScope(string name)
        {
            if(!table.TryGetValue(name, out var results)){
                if(Parent != null){
                    return Parent.GetSymbolInAnyScope(name);
                }else{
                    if(NativeMapping.TryGetValue(name, out var result))
                        return result;
                    else
                        return null;
                }
            }

            if(results.Count > 1)
                throw new InvalidOperationException("There are more than 1 candidate for '" + name + "'");

            return results.FirstOrDefault();
        }

        /// <summary>
        /// Gets the corresponding <see cref="Identifier"/> node to `name` in parent scopes that also matches to `type`.
        /// </summary>
        /// <returns>The symbol in any scope.</returns>
        /// <param name="name">Name.</param>
        /// <param name="type">Type.</param>
        public Identifier GetSymbolInAnyScope(string name, AstType type)
        {
            if(!table.TryGetValue(name, out var results)){
                if(Parent != null){
                    return Parent.GetSymbolInAnyScope(name, type);
                }else{
                    if(NativeMapping.TryGetValue(name, out var result))
                        return result;
                    else
                        return null;
                }
            }

            return results.Where(ident => ident.Type.IsMatch(type))
                          .Select(ident => ident)
                          .FirstOrDefault();
        }

        /// <summary>
        /// Gets the corresponding <see cref="Expresso.Ast.Identifier"/> node to `name` except for the natives in any parent scopes.
        /// </summary>
        /// <returns>The symbol in any scope without native.</returns>
        /// <param name="name">Name.</param>
        public Identifier GetSymbolInAnyScopeWithoutNative(string name, out bool nativeSearched)
        {
            nativeSearched = false;
            if(!table.TryGetValue(name, out var results)){
                if(Parent != null){
                    return Parent.GetSymbolInAnyScopeWithoutNative(name, out nativeSearched);
                }else{
                    nativeSearched = true;
                    return null;
                }
            }

            if(results.Count > 1)
                throw new InvalidOperationException("There are more than 1 candidate for '" + name + "'");

            return results.FirstOrDefault();
        }

        /// <summary>
        /// Gets the number of variables declared in the current scope.
        /// </summary>
        /*public int CountNames(Func<Identifier, bool> pred)
        {
            if(pred != null){
                int num_type_names = type_table.Values.Count(pred);
                int num_names = table.Values.Count(pred);
                return num_type_names + num_names;
            }else{
                return type_table.Count + table.Count;
            }
        }*/

        /// <summary>
        /// Counts up identifiers that satisfy the specified predicate function.
        /// </summary>
        /// <param name="pred">Pred.</param>
        /*public int Count(Func<Identifier, bool> pred)
        {
            if(pred != null)
                return table.Values.Count(pred);
            else
                return table.Count;
        }*/

        /// <summary>
        /// Counts all the variables that reside in this scope.
        /// </summary>
        /// <returns>The variables.</returns>
        /*public int CountVariables()
        {
            return Count(null);
        }*/

        public void AddScope(ClassType typeKind = ClassType.NotType)
        {
            var child = new SymbolTable(typeKind){
                Parent = this
            };
            Children.Add(child);
        }

        /// <summary>
        /// Adds external symbols as a new scope.
        /// It will put functions and variables under the external symbol table
        /// and will put external types on the 'programRoot' table, modifying the name.
        /// </summary>
        /// <param name="externalTable">External table.</param>
        /// <param name="aliasName">Alias name.</param>
        public void AddExternalSymbols(SymbolTable externalTable, string aliasName)
        {
            Debug.Assert(Name == "programRoot", "External symbols must be added on 'programRoot'.");

            var cloned = externalTable.Clone();
            var imported_name = TypeTablePrefix + aliasName;
            cloned.Name = imported_name;

            foreach(var symbol in cloned.Symbols){
                if(symbol.Type is FunctionType func_type){
                    var return_type = func_type.ReturnType;
                    var return_type_table = externalTable.GetTypeTable(return_type.IdentifierNode.Type.IsNull ? return_type.Name : return_type.IdentifierNode.Type.Name);
                    if(!return_type_table.IsNetType && externalTable.GetTypeSymbol(return_type.Name) != null)
                        return_type.IdentifierNode.Type = AstType.MakeSimpleType(aliasName + "::" + return_type.Name);
                }
            }

            foreach(var external_type_table in externalTable.Children){
                if(external_type_table.TypeKind != ClassType.Class && external_type_table.TypeKind != ClassType.Interface)
                    continue;
                
                var cloned2 = external_type_table.Clone();
                cloned2.Name = TypeTablePrefix + aliasName + "::" + external_type_table.Name.Substring(TypeTablePrefix.Length);
                cloned2.Parent = this;
                Children.Add(cloned2);
            }

            var tmp = this;
            while(tmp.Parent != null)
                tmp = tmp.Parent;

            cloned.Parent = tmp;
            tmp.Children.Add(cloned);
        }

        /// <summary>
        /// Adds external tables and symbols with certain paths to this table with alias names.
        /// It will put external symbols on 'programRoot'.
        /// </summary>
        /// <param name="externalTable">External table.</param>
        /// <param name="importPaths">Import paths.</param>
        /// <param name="aliasTokens">Alias tokens.</param>
        public void AddExternalSymbols(SymbolTable externalTable, IEnumerable<Identifier> importPaths, IEnumerable<Identifier> aliasTokens)
        {
            Debug.Assert(Name == "programRoot", "External tables must be added on 'programRoot'.");

            var target_table = this;

            foreach(var pair in importPaths.Zip(aliasTokens, (l, r) => new Tuple<Identifier, Identifier>(l, r))){
                var last_index = pair.Item1.Name.LastIndexOf("::", StringComparison.CurrentCulture);
                var target_name = (last_index == -1) ? pair.Item1.Name : pair.Item1.Name.Substring(last_index + "::".Length);
                var external_target_table = externalTable.GetTypeTable(target_name);

                if(external_target_table == null){
                    var target_name2 = target_name.Substring(target_name.LastIndexOf(".", StringComparison.CurrentCulture) + ".".Length);
                    var symbol = externalTable.GetSymbol(target_name2);
                    if(symbol == null){
                        throw new ParserException(
                            "Error ES0103: An external symbol '{0}' isn't found.",
                            pair.Item1,
                            pair.Item1.ToString()
                        );
                    }else{
                        if(!symbol.Modifiers.HasFlag(Modifiers.Export))
                            ReportExportMissingError(pair.Item1);

                        pair.Item2.IdentifierId = symbol.IdentifierId;
                        target_table.AddSymbol(pair.Item2.Name, symbol);
                    }
                }else{
                    var type_symbol = externalTable.GetTypeSymbol(target_name);
                    if(!type_symbol.Modifiers.HasFlag(Modifiers.Export))
                        ReportExportMissingError(pair.Item1);
                    
                    var cloned = external_target_table.Clone();
                    cloned.Parent = target_table;
                    target_table.Children.Add(cloned);

                    pair.Item1.IdentifierId = type_symbol.IdentifierId;
                    pair.Item2.IdentifierId = type_symbol.IdentifierId;
                }
            }
        }

        /// <summary>
        /// Adds a native symbol's table.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        public void AddNativeSymbolTable(Identifier symbol)
        {
            if(Parent.Name != "root")
                throw new InvalidOperationException(string.Format("Expected to call this method on programRoot but you called it on `{0}`", Name));
            
            ExpressoCompilerHelpers.AddNativeSymbolTable(symbol, Parent);
        }

        public override string ToString()
        {
            return string.Format("<SymbolTable`{0}: symbolsCount={1} typeSymbolsCount={2} childrenCount={3} type={4}>", Name, Symbols.Count(), type_table.Values.Count(), Children.Count, TypeKind);
        }

        /// <summary>
        /// Clones this instance so it has the same values as this instance.
        /// </summary>
        /// <returns>The clone.</returns>
        public SymbolTable Clone()
        {
            var cloned = new SymbolTable();

            cloned.Name = Name;
            foreach(var key in table.Keys){
                if(!table.TryGetValue(key, out var value))
                    return null;
                else
                    cloned.table.AddRange(key, value);
            }

            foreach(var key in type_table.Keys){
                if(!type_table.TryGetValue(key, out var value))
                    return null;
                else
                    cloned.type_table.Add(key, value);
            }

            foreach(var child in Children){
                var cloned_child = child.Clone();
                cloned_child.Parent = cloned;
                cloned.Children.Add(cloned_child);
            }

            return cloned;
        }

        static void ReportExportMissingError(Identifier ident)
        {
            throw new ParserException(
                "Error ES3303: '{0}' doesn't have the export flag.\nYou can't import an unexported item.",
                ident,
                ident.Name
            );
        }
    }
}
