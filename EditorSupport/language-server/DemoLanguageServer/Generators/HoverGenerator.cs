//
// HoverGenerator.cs
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
using Expresso;
using Expresso.Ast;
using Expresso.Resolver;
using Expresso.TypeSystem;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using LanguageServer.VsCode.Contracts;

namespace ExpressoLanguageServer.Generators
{
    internal static class HoverGenerator
    {
        internal static Hover GenerateHover(ExpressoAst ast, IProjectContent projectContent, ExpressoUnresolvedFile file, Position position)
        {
            var compilation = projectContent.CreateCompilation();

            var location = new TextLocation(position.Line, position.Character);
            var hovered_node = ast.GetNodeAt(location);
            var ast_resolver = new ExpressoAstResolver(new ExpressoResolver(compilation), ast, file);
            var rr = ast_resolver.Resolve(hovered_node);
            if(rr is InvocationResolveResult irr){
                var contents = string.Format("{0}({1}) -> {2}", irr.Member.Name, GeneratorHelpers.StringifyResolveResults(irr.Arguments), irr.Member.ReturnType.Name);
                return new Hover{Contents = contents};
            }

            return null;
        }
    }
}
