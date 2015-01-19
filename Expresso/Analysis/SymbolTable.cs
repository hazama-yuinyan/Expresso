using System;
using System.Collections.Generic;
using Expresso.Compiler.Meta;


namespace Expresso.Ast.Analysis
{
    /// <summary>
    /// Represents the symbol table. The symbol table is represented by a bi-directional linked list
    /// with each node representing one scope(one SymbolTable instance).
    /// In Expresso, there are 2 kinds of namespaces.
    /// One is for types, and the other is for local variables, parameters, function names and method names.
    /// </summary>
    public class SymbolTable
    {
        Dictionary<string, AstType> type_table, table;

        public SymbolTable Parent{
            get; set;
        }

        public SymbolTable Child{
            get; set;
        }

        public SymbolTable()
        {
            type_table = new Dictionary<string, AstType>();
            table = new Dictionary<string, AstType>();
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
        /// Determines whether `name` represents any symbol name.
        /// </summary>
        /// <returns><c>true</c> if `name` is declared as a symbol name; otherwise, <c>false</c>.</returns>
        /// <param name="name">The name to test.</param>
        public bool IsSymbol(string name)
        {
            bool has_in_this = HasTypeSymbol(name) || HasSymbol(name);
            if(!has_in_this)
                return Parent.IsSymbol(name);
            else
                return has_in_this;
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
            type_table.Add(name, type);
        }

        /// <summary>
        /// Adds a new name to the symbol scope.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="type">Type.</param>
        public void AddSymbol(string name, AstType type)
        {
            table.Add(name, type);
        }

        /// <summary>
        /// Gets the corresponding <see cref="Expresso.Ast.AstType"/> node to `name`.
        /// </summary>
        /// <returns>The symbol type.</returns>
        /// <param name="name">Name.</param>
        public AstType GetSymbolType(string name)
        {
            AstType result;
            if(!table.TryGetValue(name, out result)){
                throw new ParserException(
                    "{0} turns out not to be declared in type check phase!\nSomething wrong is happening!",
                    name
                );
            }
            return result;
        }

        public void AddScope()
        {
            var child = new SymbolTable();
            child.Parent = this;
            Child = child;
        }

        public void RemoveScope()
        {
            Child.Parent = null;
            Child = null;
        }
    }
}
