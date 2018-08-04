using System;
namespace Expresso.CodeGen
{
    /// <summary>
    /// Represents a document entry for the debug table.
    /// </summary>
    public struct DebugDocument
    {
        public string FilePath{
            get; set;
        }

        public Guid HashAlgorithm{
            get; set;
        }

        public Guid Hash{
            get; set;
        }

        public Guid LanguageGuid{
            get; set;
        }

        public DebugDocument(string filePath, Guid hashAlgorithm, Guid hash, Guid languageGuid)
        {
            FilePath = filePath;
            HashAlgorithm = hashAlgorithm;
            Hash = hash;
            LanguageGuid = languageGuid;
        }
    }
}
