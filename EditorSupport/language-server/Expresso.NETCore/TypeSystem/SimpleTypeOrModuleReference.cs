//
// SimpleTypeOrModuleReference.cs
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
using Expresso.Resolver;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;

namespace Expresso.TypeSystem
{
    /// <summary>
    /// Represents a simple Expresso name(a single non-qualified identifier with an optional list of type arguments).
    /// </summary>
    [Serializable]
    public class SimpleTypeOrModuleReference : TypeOrModuleReference, ISupportsInterning
    {
        readonly string identifier;
        readonly IList<ITypeReference> type_arguments;
        readonly NameLookupMode lookup_mode;

        public string Identifier => identifier;
        public IList<ITypeReference> TypeArguments => type_arguments;
        public NameLookupMode LookupMode => lookup_mode;

        public SimpleTypeOrModuleReference(string identifier, IList<ITypeReference> typeArgs, NameLookupMode lookupMode)
        {
            if(identifier == null)
                throw new ArgumentNullException(nameof(identifier));
            
            this.identifier = identifier;
            type_arguments = typeArgs ?? EmptyList<ITypeReference>.Instance;
            lookup_mode = lookupMode;
        }

        /// <summary>
        /// Adds a suffix to the identifier.
        /// Does not modify the existing type reference, but returns a new one.
        /// </summary>
        /// <returns>The suffix.</returns>
        /// <param name="suffix">Suffix.</param>
        public SimpleTypeOrModuleReference AddSuffix(string suffix)
        {
            return new SimpleTypeOrModuleReference(identifier + suffix, TypeArguments, LookupMode);
        }

        public override ResolveResult Resolve(ExpressoResolver resolver)
        {
            var type_args = TypeArguments.Resolve(resolver.CurrentTypeResolveContext);
            return resolver.LookupSimpleNameOrTypeName(identifier, type_args, lookup_mode);
        }

        public override IType ResolveType(ExpressoResolver resolver)
        {
            if(Resolve(resolver) is TypeResolveResult trr)
                return trr.Type;
            else
                return new UnknownType(null, Identifier, type_arguments.Count);
        }

        bool ISupportsInterning.EqualsForInterning(ISupportsInterning other)
        {
            var o = other as SimpleTypeOrModuleReference;
            return o != null && identifier == o.identifier && type_arguments == o.type_arguments && lookup_mode == o.lookup_mode;
        }

        int ISupportsInterning.GetHashCodeForInterning()
        {
            int hash_code = 0;
            unchecked{
                hash_code += 1000000021 * identifier.GetHashCode();
                hash_code += 1000000033 * type_arguments.GetHashCode();
                hash_code += 1000000087 * (int)lookup_mode;
            }
            return hash_code;
        }
    }
}
