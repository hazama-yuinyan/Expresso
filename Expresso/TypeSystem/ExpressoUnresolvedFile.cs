using System;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using System.Collections.Generic;
using ICSharpCode.NRefactory;


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

        public ExpressoUnresolvedFile()
        {
        }

        #region IUnresolvedFile implementation

        public IUnresolvedTypeDefinition GetTopLevelTypeDefinition(TextLocation location)
        {
            throw new NotImplementedException();
        }

        public IUnresolvedTypeDefinition GetInnermostTypeDefinition(TextLocation location)
        {
            throw new NotImplementedException();
        }

        public IUnresolvedMember GetMember(TextLocation location)
        {
            throw new NotImplementedException();
        }

        public string FileName{
            get{
                return file_name;
            }

            set{
                file_name = value;
            }
        }

        public DateTime? LastWriteTime{
            get{
                throw new NotImplementedException();
            }
            set{
                throw new NotImplementedException();
            }
        }

        public IList<IUnresolvedTypeDefinition> TopLevelTypeDefinitions{
            get{
                return unresolved_typedefs;
            }
        }

        public IList<IUnresolvedAttribute> AssemblyAttributes{
            get{
                throw new NotImplementedException();
            }
        }

        public IList<IUnresolvedAttribute> ModuleAttributes{
            get{
                throw new NotImplementedException();
            }
        }

        public IList<Error> Errors{
            get{
                return errors;
            }
        }

        #endregion
    }
}

