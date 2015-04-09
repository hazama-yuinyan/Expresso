using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;


namespace Expresso.Ast.Analysis
{
    /// <summary>
    /// Represents the symbol table. The symbol table is represented by a n-branched tree
    /// with each node representing one scope(one SymbolTable instance).
    /// In Expresso, there are 2 kinds of namespaces.
    /// One is for types, and the other is for local variables, parameters, function names and method names.
    /// </summary>
    public class SymbolTable : ISerializable
    {
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
        public int NumOfSymbols{
            get{return Symbols.Count();}
        }

        public SymbolTable()
        {
            type_table = new Dictionary<string, Identifier>();
            table = new Dictionary<string, Identifier>();
            Children = new List<SymbolTable>();
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
            var table = this;
            var class_name = "class " + name;
            int child_counter = 0;
            while(table != null){
                if(table.Name.StartsWith(class_name))
                    return table;

                if(child_counter >= table.Parent.Children.Count){
                    child_counter = 0;
                    table = table.Parent;
                }else{
                    table = table.Parent.Children[child_counter++];
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

        public void AddSymbol(string name, Identifier ident)
        {
            try{
                table.Add(name, ident);
            }
            catch(ArgumentException){
                throw new ParserException("The name `{0}` is already defined in the current scope {1}.", name, Name);
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
                if(Parent != null)
                    return Parent.GetSymbolInAnyScope(name);
                else
                    return null;
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

        public override string ToString()
        {
            return string.Format("<SymbolTable`{0}: {1}>", Name, Symbols.Count());
        }
    }
}
