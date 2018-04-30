//
// ExpressoProjectContent.cs
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
using Expresso.CodeGen;
using Expresso.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using ICSharpCode.NRefactory.Utils;

namespace Expresso
{
    /// <summary>
    /// Represents a project content of Expresso
    /// </summary>
    public class ExpressoProjectContent : IProjectContent
    {
        string assembly_name;
        string full_assembly_name;
        string project_file_name;
        string location;
        Dictionary<string, IUnresolvedFile> unresolved_files;
        List<IAssemblyReference> assembly_references;
        ExpressoCompilerOptions compiler_options;

        public ExpressoProjectContent()
        {
            unresolved_files = new Dictionary<string, IUnresolvedFile>(Platform.FileNameComparer);
            assembly_references = new List<IAssemblyReference>();
            compiler_options = new ExpressoCompilerOptions();
        }

        public ExpressoProjectContent(ExpressoProjectContent pc)
        {
            assembly_name = pc.assembly_name;
            full_assembly_name = pc.full_assembly_name;
            project_file_name = pc.project_file_name;
            location = pc.location;
            unresolved_files = new Dictionary<string, IUnresolvedFile>(pc.unresolved_files, Platform.FileNameComparer);
            assembly_references = new List<IAssemblyReference>(pc.assembly_references);
            compiler_options = pc.compiler_options;
            compiler_options.Freeze();
        }

        public string ProjectFileName => project_file_name;

        public IEnumerable<IUnresolvedFile> Files => unresolved_files.Values;

        public IEnumerable<IAssemblyReference> AssemblyReferences => assembly_references;

        public object CompilerSettings => compiler_options;

        public string AssemblyName => assembly_name;

        public string FullAssemblyName => full_assembly_name;

        public string Location => location;

        public IEnumerable<IUnresolvedAttribute> AssemblyAttributes => Files.SelectMany(f => f.AssemblyAttributes);

        public IEnumerable<IUnresolvedAttribute> ModuleAttributes => Files.SelectMany(f => f.ModuleAttributes);

        public IEnumerable<IUnresolvedTypeDefinition> TopLevelTypeDefinitions => Files.SelectMany(f => f.TopLevelTypeDefinitions);

        protected virtual ExpressoProjectContent Clone()
        {
            return new ExpressoProjectContent(this);
        }

        public IProjectContent AddAssemblyReferences(IEnumerable<IAssemblyReference> references)
        {
            return AddAssemblyReferences(references.ToArray());
        }

        public IProjectContent AddAssemblyReferences(params IAssemblyReference[] references)
        {
            var pc = Clone();
            pc.assembly_references.AddRange(references);
            return pc;
        }

        /// <summary>
        /// Adds the specified files to the project content.
        /// If a file with the same name already exists, updated the existing file.
        /// </summary>
        /// <returns>The or update files.</returns>
        /// <param name="newFiles">New files.</param>
        public IProjectContent AddOrUpdateFiles(IEnumerable<IUnresolvedFile> newFiles)
        {
            var pc = Clone();
            foreach(var file in newFiles)
                pc.unresolved_files[file.FileName] = file;

            return pc;
        }

        /// <summary>
        /// Adds the specified files to the project content.
        /// If a file with the same name already exists, this method updates the existing file.
        /// </summary>
        /// <returns>The or update files.</returns>
        /// <param name="newFiles">New files.</param>
        public IProjectContent AddOrUpdateFiles(params IUnresolvedFile[] newFiles)
        {
            return AddOrUpdateFiles((IEnumerable<IUnresolvedFile>)newFiles);
        }

        public virtual ICompilation CreateCompilation()
        {
            var solution_snapshot = new DefaultSolutionSnapshot();
            var compilation = new SimpleCompilation(solution_snapshot, this, AssemblyReferences);
            solution_snapshot.AddCompilation(this, compilation);
            return compilation;
        }

        public virtual ICompilation CreateCompilation(ISolutionSnapshot solutionSnapshot)
        {
            return new SimpleCompilation(solutionSnapshot, this, AssemblyReferences);
        }

        public IUnresolvedFile GetFile(string fileName)
        {
            return unresolved_files.TryGetValue(fileName, out var file) ? file : null;
        }

        public IProjectContent RemoveAssemblyReferences(IEnumerable<IAssemblyReference> references)
        {
            return RemoveAssemblyReferences(references.ToArray());
        }

        public IProjectContent RemoveAssemblyReferences(params IAssemblyReference[] references)
        {
            var pc = Clone();
            foreach(var r in references)
                pc.assembly_references.Remove(r);

            return pc;
        }

        /// <summary>
        /// Removes the files with specified names.
        /// </summary>
        /// <returns>The files.</returns>
        /// <param name="fileNames">File names.</param>
        public IProjectContent RemoveFiles(IEnumerable<string> fileNames)
        {
            var pc = Clone();
            foreach(var file_name in fileNames)
                pc.unresolved_files.Remove(file_name);

            return pc;
        }

        public IProjectContent RemoveFiles(params string[] fileNames)
        {
            return RemoveFiles((IEnumerable<string>)fileNames);
        }

        public IAssembly Resolve(ITypeResolveContext context)
        {
            if(context == null)
                throw new ArgumentNullException(nameof(context));

            var cache = context.Compilation.CacheManager;
            var asm = (IAssembly)cache.GetShared(this);
            if(asm != null){
                return asm;
            }else{
                asm = new ExpressoAssembly(context.Compilation, this);
                return (IAssembly)cache.GetOrAddShared(this, asm);
            }
        }

        /// <summary>
        /// Sets both the short and the full assembly name.
        /// </summary>
        /// <returns>The assembly name.</returns>
        /// <param name="newAssemblyName">New assembly name.</param>
        public IProjectContent SetAssemblyName(string newAssemblyName)
        {
            var pc = Clone();
            pc.full_assembly_name = newAssemblyName;
            int pos = (newAssemblyName != null) ? newAssemblyName.IndexOf(',') : -1;
            pc.assembly_name = (pos < 0) ? newAssemblyName : newAssemblyName.Substring(0, pos);
            return pc;
        }

        public IProjectContent SetCompilerSettings(object compilerSettings)
        {
            if(!(compilerSettings is ExpressoCompilerOptions))
                throw new ArgumentException("Options must be an instance of " + typeof(ExpressoCompilerOptions).FullName, nameof(compilerSettings));

            var pc = Clone();
            pc.compiler_options = (ExpressoCompilerOptions)compilerSettings;
            pc.compiler_options.Freeze();
            return pc;
        }

        public IProjectContent SetLocation(string newLocation)
        {
            var pc = Clone();
            pc.location = newLocation;
            return pc;
        }

        public IProjectContent SetProjectFileName(string newProjectFileName)
        {
            var pc = Clone();
            pc.project_file_name = newProjectFileName;
            return pc;
        }

        [Obsolete("Use RemoveFiles/AddOrUpdateFiles instead")]
        public IProjectContent UpdateProjectContent(IUnresolvedFile oldFile, IUnresolvedFile newFile)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use RemoveFiles/AddOrUpdateFiles instead")]
        public IProjectContent UpdateProjectContent(IEnumerable<IUnresolvedFile> oldFiles, IEnumerable<IUnresolvedFile> newFiles)
        {
            throw new NotImplementedException();
        }
    }
}
