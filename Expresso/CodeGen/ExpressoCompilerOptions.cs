using System;
using System.Collections.Generic;


namespace Expresso.CodeGen
{
    /// <summary>
    /// Represents build target mode.
    /// </summary>
    public enum BuildType
    {
        Debug,
        Release
    }

    /// <summary>
    /// Represents and contains some options for Expresso compiler.
    /// </summary>
    public class ExpressoCompilerOptions
    {
        /// <summary>
        /// Specifies the path to the libraries directory.
        /// </summary>
        public List<string> LibraryPaths{
            get; set;
        }

        /// <summary>
        /// Specifies the build type.
        /// In debug build, no optimizations will be performed.
        /// </summary>
        public BuildType BuildType{
            get; set;
        }

        /// <summary>
        /// Specifies the output executable or assembly path.
        /// </summary>
        public string OutputPath{
            get; set;
        }
    }
}

