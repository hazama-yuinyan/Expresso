

namespace Expresso.Ast.Analysis
{
    /// <summary>
    /// A UniqueIdGenerator generates a unique id throughout the whole program at a time.
    /// </summary>
    public static class UniqueIdGenerator
    {
        static uint Id = 1;

        /// <summary>
        /// Peeks at the next unique id.
        /// </summary>
        public static uint CurrentId => Id;

        /// <summary>
        /// Assigns the next id to the given identifier.
        /// </summary>
        public static void DefineNewId(Identifier ident)
        {
            if(ident.IdentifierId != 0)
                return;
            
            ident.IdentifierId = Id++;
        }
    }
}

