//
// ExpressoAssembly.cs
//
// Author:
//       train12 <kotonechan@live.jp>
//
// Copyright (c) 2018 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using ICSharpCode.NRefactory.Utils;

namespace Expresso.TypeSystem
{
    /// <summary>
    /// Represents an assembly of Expresso.
    /// </summary>
    public class ExpressoAssembly : IAssembly
    {
        readonly ICompilation compilation;
        readonly ITypeResolveContext context;
        readonly ExpressoProjectContent project_content;
        IList<IAttribute> assembly_attributes;
        IList<IAttribute> module_attributes;

        internal ExpressoAssembly(ICompilation compilation, ExpressoProjectContent projectContent)
        {
            this.compilation = compilation;
            project_content = projectContent;
            context = new SimpleTypeResolveContext(this);
        }

        public IUnresolvedAssembly UnresolvedAssembly => project_content;

        public bool IsMainAssembly => compilation.MainAssembly == this;

        public string AssemblyName => project_content.AssemblyName;

        public string FullAssemblyName => project_content.FullAssemblyName;

        public IList<IAttribute> AssemblyAttributes => assembly_attributes;

        public IList<IAttribute> ModuleAttributes => module_attributes;

        public INamespace RootNamespace => throw new NotImplementedException();

        public IEnumerable<ITypeDefinition> TopLevelTypeDefinitions => GetTypes().Values;

        public ICompilation Compilation => compilation;

        Dictionary<TopLevelTypeName, ITypeDefinition> type_dict;

        Dictionary<TopLevelTypeName, ITypeDefinition> GetTypes()
        {
            var dict = LazyInit.VolatileRead(ref type_dict);
            if(dict != null){
                return dict;
            }else{
                var comparer = TopLevelTypeNameComparer.Ordinal;
                dict = project_content.TopLevelTypeDefinitions
                                      .Select(t => new Tuple<TopLevelTypeName, IUnresolvedTypeDefinition>(new TopLevelTypeName(t.Namespace, t.Name, t.TypeParameters.Count), t))
                                      .ToDictionary(t => t.Item1, t => CreateResolvedTypeDefinition(t.Item2), comparer);
                return LazyInit.GetOrSet(ref type_dict, dict);
            }
        }

        ITypeDefinition CreateResolvedTypeDefinition(IUnresolvedTypeDefinition type)
        {
            return new DefaultResolvedTypeDefinition(context, type);
        }

        public ITypeDefinition GetTypeDefinition(TopLevelTypeName topLevelTypeName)
        {
            return GetTypes().TryGetValue(topLevelTypeName, out var t) ? t : null;
        }

        public override string ToString()
        {
            return "[ExpressoAssembly " + AssemblyName + "]";
        }

        public bool InternalsVisibleTo(IAssembly assembly)
        {
            if(this == assembly)
                return true;

            foreach(var short_name in GetInternalsVisibleTo()){
                if(assembly.AssemblyName == short_name)
                    return true;
            }

            return false;
                
        }

        IEnumerable<string> GetInternalsVisibleTo()
        {
            return new []{""};
        }
    }
}
