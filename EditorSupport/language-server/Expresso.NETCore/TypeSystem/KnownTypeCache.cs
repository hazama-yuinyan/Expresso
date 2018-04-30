//
// KnownTypeCache.cs
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
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using ICSharpCode.NRefactory.Utils;

namespace Expresso.TypeSystem
{
    /// <summary>
    /// Cache for <see cref="KnownTypeReference"/>s.
    /// </summary>
    public static class KnownTypeCache
    {
        static readonly IType[] knownTypes = new IType[KnownTypeReference.KnownTypeCount];

        public static IType FindType(ICompilation compilation, KnownTypeCode knownTypeCode)
        {
            var type = LazyInit.VolatileRead(ref knownTypes[(int)knownTypeCode]);
            if(type != null)
                return type;

            return LazyInit.GetOrSet(ref knownTypes[(int)knownTypeCode], SearchType(compilation, knownTypeCode));
        }

        static IType SearchType(ICompilation compilation, KnownTypeCode typeCode)
        {
            var type_ref = KnownTypeReference.Get(typeCode);
            if(type_ref == null)
                return SpecialType.UnknownType;

            var type_name = new TopLevelTypeName(type_ref.Namespace, type_ref.Name, type_ref.TypeParameterCount);
            foreach(var asm in compilation.Assemblies){
                var type_def = asm.GetTypeDefinition(type_name);
                if(type_def != null)
                    return type_def;
            }

            return new UnknownType(type_name);
        }
    }
}
