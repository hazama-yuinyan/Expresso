using System;


namespace Expresso.CodeGen
{
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
        public string Path{
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

