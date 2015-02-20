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

        public SymbolTable Parent{
            get; set;
        }

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

        public SymbolTable()
        {
            type_table = new Dictionary<string, Identifier>();
            table = new Dictionary<string, Identifier>();
            Children = new List<SymbolTable>();
        }

        #region ISerializable implementation

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach(var entry in type_table)
                info.AddValue(entry.Key, entry.Value.Type, typeof(AstType));

            foreach(var entry2 in table)
                info.AddValue(entry2.Key, entry2.Value.Type, typeof(AstType));
        }

        #endregion

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
                throw new ParserException("The name `{0}` is already defined in the current scope.", name);
            }
        }

        /// <summary>
        /// Gets the corresponding <see cref="Expresso.Ast.Identifier"/> node to type `name`.
        /// </summary>
        /// <returns>The type symbol.</returns>
        /// <param name="name">Name.</param>
        public Identifier GetTypeSymbol(string name)
        {
            Identifier result;
            if(!type_table.TryGetValue(name, out result)){
                throw new ParserException(
                    "Type '{0}' turns out not be declared in the current scope!\nSomething wrong is happening!",
                    name
                );
            }

            return result;
        }

        /// <summary>
        /// Gets the corresponding <see cref="Expresso.Ast.Identifier"/> node to `name`.
        /// </summary>
        /// <returns>The symbol.</returns>
        /// <param name="name">Name.</param>
        public Identifier GetSymbol(string name)
        {
            Identifier result;
            if(!table.TryGetValue(name, out result)){
                throw new ParserException(
                    "{0} turns out not to be declared in type check phase!\nSomething wrong is happening!",
                    name
                );
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
    }
}
