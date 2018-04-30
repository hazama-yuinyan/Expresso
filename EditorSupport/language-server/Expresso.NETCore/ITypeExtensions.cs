//
// ITypeExtensions.cs
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
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;

namespace Expresso.TypeSystem
{
    /// <summary>
    /// Contains extension methods for <see cref="IType"/>.
    /// </summary>
    public static class ITypeExtensions
    {
        /// <summary>
        /// Gets the method with the specified name, providing a similar API to one in <see cref="Type"/>.
        /// </summary>
        /// <returns>The method.</returns>
        /// <param name="type">Type.</param>
        /// <param name="methodName">Method name.</param>
        public static IMethod GetMethod(this IType type, string methodName)
        {
            var possibly_methods = type.GetMethods(m => m.Name == methodName);
            if(possibly_methods.Count() > 1)
                throw new FormattedException("Ambiguous results: {0}.", methodName);

            return possibly_methods.FirstOrDefault();
        }

        /// <summary>
        /// Gets the method with the specified name and parameter types, providing a similar API to one in <see cref="Type"/>.
        /// </summary>
        /// <returns>The method.</returns>
        /// <param name="type">Type.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="parameterTypes">Parameter types.</param>
        public static IMethod GetMethod(this IType type, string methodName, ITypeReference[] parameterTypes)
        {
            return type.GetMethods(m => {
                return m.Name == methodName && m.Parameters.Zip(parameterTypes, (param, paramType) => new {paramType = param.Type, type = paramType})
                        .All(anonymousStruct => anonymousStruct.paramType == anonymousStruct.type);
            }).FirstOrDefault();
        }

        /// <summary>
        /// Gets the field with the specified name, providing a similar API to one in <see cref="Type"/>.
        /// </summary>
        /// <returns>The field.</returns>
        /// <param name="type">Type.</param>
        /// <param name="fieldName">Field name.</param>
        public static IField GetField(this IType type, string fieldName)
        {
            return type.GetFields(f => f.Name == fieldName).FirstOrDefault();
        }

        /// <summary>
        /// Gets the property with the specified name, providing a similar API to one in <see cref="Type"/>.
        /// </summary>
        /// <returns>The property.</returns>
        /// <param name="type">Type.</param>
        /// <param name="propertyName">Property name.</param>
        public static IProperty GetProperty(this IType type, string propertyName)
        {
            return type.GetProperties(p => p.Name == propertyName).FirstOrDefault();
        }
    }
}
