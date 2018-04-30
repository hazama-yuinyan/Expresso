//
// Symbol.cs
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
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;

namespace Expresso.TypeSystem
{
    /// <summary>
    /// Represents a symbol in the type system.
    /// </summary>
    public sealed class Symbol
    {
        /// <summary>
        /// Represents a local variable or a parameter.
        /// </summary>
        /// <value>The variable.</value>
        public IVariable Variable{
            get; set;
        }

        /// <summary>
        /// Represents a method(including functions).
        /// </summary>
        /// <value>The method.</value>
        public IMethod Method{
            get; set;
        }

        /// <summary>
        /// Represents a type.
        /// </summary>
        /// <value>The type.</value>
        public IType Type{
            get; set;
        }

        /// <summary>
        /// Represents a field or a property.
        /// </summary>
        /// <value>The property or field.</value>
        public IMember PropertyOrField{
            get; set;
        }

        /// <summary>
        /// Represents the <see cref="ResolveResult"/> associated with the symbol.
        /// </summary>
        /// <value>The resolve result.</value>
        public ResolveResult ResolveResult{
            get; set;
        }
    }
}
