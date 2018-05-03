//
// GeneratorHelpers..cs
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
using System.Text;
using ICSharpCode.NRefactory.Semantics;

namespace ExpressoLanguageServer.Generators
{
    /// <summary>
    /// Contains helper methods for <see cref="HoverGenerator"/>.
    /// </summary>
    public static class GeneratorHelpers
    {
        /// <summary>
        /// Stringifies the list.
        /// </summary>
        /// <returns>The list.</returns>
        /// <param name="source">Source.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static string StringifyList<T>(IEnumerable<T> source)
        {
            if(!source.Any())
                return "";
            
            var builder = new StringBuilder(source.First().ToString());
            foreach(var item in source.Skip(1)){
                builder.Append(", ");
                builder.Append(item);
            }

            return builder.ToString();
        }

        public static string StringifyResolveResults(IEnumerable<ResolveResult> source)
        {
            if(!source.Any())
                return "";
            
            var builder = new StringBuilder(source.First().ToString());
            foreach(var result in source){
                builder.Append(", ");
                builder.Append(result.ToString());
            }

            return builder.ToString();
        }
    }
}
