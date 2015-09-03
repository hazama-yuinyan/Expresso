using System;
<<<<<<< HEAD
=======
using System.Collections.Generic;
>>>>>>> 1dd21683d86e185bb8de4772c4351cc7f1620cb6


namespace Expresso.CodeGen
{
<<<<<<< HEAD
=======
    /// <summary>
    /// Represents build target mode.
    /// </summary>
>>>>>>> 1dd21683d86e185bb8de4772c4351cc7f1620cb6
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
<<<<<<< HEAD
        public string Path{
=======
        public List<string> LibraryPaths{
>>>>>>> 1dd21683d86e185bb8de4772c4351cc7f1620cb6
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

