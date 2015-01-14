using System;
using System.Collections.Generic;
using Expresso.Compiler.Meta;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents the symbol table. A symbol table is represented by a binary tree with each node
    /// representing one scope.
    /// </summary>
    public class SymbolTable
    {
        Dictionary<string, AstType> table;

        public SymbolTable Parent{
            get; set;
        }

        public SymbolTable Child{
            get; set;
        }

        public bool HasSymbol(string name)
        {
            return table.ContainsKey(name);
        }

        public bool IsSymbol(string name)
        {
            bool has_in_this = HasSymbol(name);
            if(!has_in_this)
                return Parent.IsSymbol(name);
            else
                return has_in_this;
        }

        public void AddSymbol(string name, AstType type)
        {
            table.Add(name, type);
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

