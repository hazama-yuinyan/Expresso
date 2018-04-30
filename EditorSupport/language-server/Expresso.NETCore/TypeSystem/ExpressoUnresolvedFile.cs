using System;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using System.Collections.Generic;
using ICSharpCode.NRefactory;
using System.Linq;

namespace Expresso.TypeSystem
{
    /// <summary>
    /// Represents a file that was parsed and converted for the type system.
    /// </summary>
    public class ExpressoUnresolvedFile : AbstractFreezable, IUnresolvedFile
    {
        string file_name = string.Empty;
        IList<IUnresolvedTypeDefinition> unresolved_typedefs = new List<IUnresolvedTypeDefinition>();

        IList<Error> errors = new List<Error>();

        #region IUnresolvedFile implementation

        public IUnresolvedTypeDefinition GetTopLevelTypeDefinition(TextLocation location)
        {
            return FindEntity(unresolved_typedefs, location);
        }

        public IUnresolvedTypeDefinition GetInnermostTypeDefinition(TextLocation location)
        {
            IUnresolvedTypeDefinition parent = null;
            var type = GetTopLevelTypeDefinition(location);
            while(type != null){
                parent = type;
                type = FindEntity(parent.NestedTypes, location);
            }

            return parent;
        }

        public IUnresolvedMember GetMember(TextLocation location)
        {
            var type = GetTopLevelTypeDefinition(location);
            if(type == null)
                return null;

            return FindEntity(type.Members, location);
        }

        public string FileName{
            get; set;
        }

        public DateTime? LastWriteTime{
            get; set;
        }

        public IList<IUnresolvedTypeDefinition> TopLevelTypeDefinitions => unresolved_typedefs;

        public IList<IUnresolvedAttribute> AssemblyAttributes => throw new NotImplementedException();

        public IList<IUnresolvedAttribute> ModuleAttributes => throw new NotImplementedException();

        public IList<Error> Errors => errors;

        #endregion

        T FindEntity<T>(IList<T> list, TextLocation location)
            where T : class, IUnresolvedEntity
        {
            foreach(var item in list){
                if(item.Region.IsInside(location))
                    return item;
            }

            return null;
        }

        public ITypeResolveContext GetTypeResolveContext(ICompilation compilation, TextLocation location)
        {
            var r_ctx = new ExpressoTypeResolveContext(compilation.MainAssembly);
            var cur_def = GetInnermostTypeDefinition(location);
            if(cur_def != null){
                var resolved_def = cur_def.Resolve(r_ctx).GetDefinition();
                if(resolved_def == null)
                    return r_ctx;

                r_ctx = r_ctx.WithCurrentTypeDefinition(resolved_def);
                var cur_member = resolved_def.Members.FirstOrDefault(m => m.BodyRegion.FileName == FileName && m.BodyRegion.Begin <= location && location <= m.BodyRegion.End);
                if(cur_member != null)
                    r_ctx = r_ctx.WithCurrentMember(cur_member);
            }

            return r_ctx;
        }
    }
}

