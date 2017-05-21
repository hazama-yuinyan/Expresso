using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ICSharpCode.NRefactory;
using Expresso.TypeSystem;


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
        static Dictionary<string, Identifier> NativeMapping;
        static string TypeTablePrefix = "type_";
        Dictionary<string, Identifier> type_table, table;

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
                return table.Values;
            }
        }

        /// <summary>
        /// Gets the number of symbols within this scope.
        /// </summary>
        public int NumberOfSymbols{
            get{return Symbols.Count();}
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
                    AstType.MakePrimitiveType("String", TextLocation.Empty)
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
                    AstType.MakePrimitiveType("String", TextLocation.Empty)
                )
            );
            println_ident.IdentifierId = 1000000001u;
            NativeMapping.Add("println", println_ident);

            var printformat_ident = AstNode.MakeIdentifier(
                "WriteLine",
                AstType.MakeFunctionType(
                    "printFormat",
                    AstType.MakeSimpleType("tuple", TextLocation.Empty),    // The void type
                    TextLocation.Empty,
                    TextLocation.Empty,
                    AstType.MakePrimitiveType("String", TextLocation.Empty)
                )
            );
            printformat_ident.IdentifierId = 1000000002u;
            NativeMapping.Add("printFormat", printformat_ident);
        }

        public SymbolTable()
        {
            type_table = new Dictionary<string, Identifier>();
            table = new Dictionary<string, Identifier>();
            Children = new List<SymbolTable>();
        }

        public static SymbolTable Create()
        {
            var table = new SymbolTable();
            table.Name = "programRoot";
            var table2 = new SymbolTable();
            table2.Name = "root";
            table.Parent = table2;
            table2.Children.Add(table);

            // TODO: Use reflection to add native symbols
            var vector_table = new SymbolTable();
            vector_table.Name = TypeTablePrefix + "vector`T";
            vector_table.AddSymbol("add", AstType.MakeFunctionType("add", AstType.MakeSimpleType("tuple", TextLocation.Empty), new List<AstType>{
                AstType.MakeParameterType("T")
            }));
            vector_table.GetSymbol("add").IdentifierId = 1000000002u;
            table2.Children.Add(vector_table);
            table2.AddTypeSymbol("vector", AstType.MakeSimpleType("vector", new List<AstType>{AstType.MakeParameterType("T")}));

            return table;
        }

        public static Identifier GetNativeSymbol(string name)
        {
            Identifier result;
            if(NativeMapping.TryGetValue(name, out result))
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
                var ast_type = entry2.Value.Type;
                info.AddValue(entry2.Key, ast_type, ast_type.GetType());
            }
        }

        #endregion

        public SymbolTable GetTypeTable(string name)
        {
            var parent = Parent;
            var tmp = parent.Children[0];
            var class_name = TypeTablePrefix + name;
            int child_counter = 1;
            while(tmp != null){
                if(tmp.Name.Equals(class_name))
                    return tmp;

                if(child_counter >= parent.Children.Count){
                    child_counter = 1;
                    parent = parent.Parent;
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

        /// <summary>
        /// Determines whether `name` is a symbol name(variables or functions).
        /// </summary>
        /// <returns><c>true</c> if `name` is a symbol name; otherwise, <c>false</c>.</returns>
        /// <param name="name">The name to test.</param>
        public bool HasSymbol(string name)
        {
            return table.ContainsKey(name);
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
            type_table.Add(name, AstNode.MakeIdentifier(name, type));
        }

        /// <summary>
        /// Adds a new name to the type namespace.
        /// Use this overload when encountered with a new type declaration.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="ident">Ident.</param>
        public void AddTypeSymbol(string name, Identifier ident)
        {
            type_table.Add(name, ident);
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
                throw new ParserException("The name `{0}` is already defined in the current scope {1}.", ident, name, Name);
            }
        }

        /// <summary>
        /// Gets the corresponding <see cref="Expresso.Ast.Identifier"/> node to type `name`.
        /// </summary>
        public Identifier GetTypeSymbol(string name)
        {
            Identifier result;
            if(!type_table.TryGetValue(name, out result))
                return null;

            return result;
        }

        /// <summary>
        /// Gets the corresponding <see cref="Expresso.Ast.Identifier"/> node to `name`.
        /// </summary>
        public Identifier GetSymbol(string name)
        {
            Identifier result;
            if(!table.TryGetValue(name, out result))
                return null;

            return result;
        }

        /// <summary>
        /// Gets the corresponding <see cref="Expresso.Ast.Identifier"/> node to `name` in any parent scopes.
        /// </summary>
        public Identifier GetSymbolInAnyScope(string name)
        {
            Identifier result;
            if(!table.TryGetValue(name, out result)){
                if(Parent != null){
                    return Parent.GetSymbolInAnyScope(name);
                }else{
                    if(NativeMapping.TryGetValue(name, out result))
                        return result;
                    else
                        return null;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the number of variables declared in the current scope.
        /// </summary>
        public int CountNames(Func<Identifier, bool> pred)
        {
            if(pred != null){
                int num_type_names = type_table.Values.Count(pred);
                int num_names = table.Values.Count(pred);
                return num_type_names + num_names;
            }else{
                return type_table.Count + table.Count;
            }
        }

        /// <summary>
        /// Counts up identifiers that satisfy the specified predicate function.
        /// </summary>
        /// <param name="pred">Pred.</param>
        public int Count(Func<Identifier, bool> pred)
        {
            if(pred != null)
                return table.Values.Count(pred);
            else
                return table.Count;
        }

        public int CountVariables()
        {
            return Count(null);
        }

        public void AddScope()
        {
            var child = new SymbolTable();
            child.Parent = this;
            Children.Add(child);
        }

        /// <summary>
        /// Adds external symbols to the global scope of this SymbolTable.
        /// </summary>
        /// <param name="externalTable">External table.</param>
        /*public void AddExternalSymbols(SymbolTable externalTable)
        {
            foreach(var external_symbol in externalTable.table)
                table.Add(external_symbol.Key, external_symbol.Value);

            foreach(var external_typesymbol in externalTable.type_table)
                type_table.Add(external_typesymbol.Key, external_typesymbol.Value);
        }*/

        /// <summary>
        /// Adds external symbols as a new scope.
        /// </summary>
        /// <param name="externalTable">External table.</param>
        /// <param name="aliasName">Alias name.</param>
        public void AddExternalSymbols(SymbolTable externalTable, string aliasName)
        {
            var cloned = externalTable.Clone();
            cloned.Name = TypeTablePrefix + aliasName;

            var tmp = this;
            while(tmp.Parent != null)
                tmp = tmp.Parent;

            cloned.Parent = tmp;
            tmp.Children.Add(cloned);
        }

        public override string ToString()
        {
            return string.Format("<SymbolTable`{0}: count={1}, childrenCount={2}>", Name, Symbols.Count(), Children.Count);
        }

        /// <summary>
        /// Clones this instance as it has the same values as this instance.
        /// </summary>
        /// <returns>The clone.</returns>
        public SymbolTable Clone()
        {
            var cloned = new SymbolTable();

            cloned.Name = Name;
            Identifier value;
            foreach(var key in table.Keys){
                if(!table.TryGetValue(key, out value))
                    return null;
                else
                    cloned.table.Add(key, value);
            }

            foreach(var key in type_table.Keys){
                if(!type_table.TryGetValue(key, out value))
                    return null;
                else
                    cloned.type_table.Add(key, value);
            }

            return cloned;
        }
    }
}
