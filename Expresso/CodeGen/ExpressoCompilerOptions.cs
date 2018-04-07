using System;


namespace Expresso.CodeGen
{
    /// <summary>
    /// Represents build target mode.
    /// </summary>
    [Flags]
    public enum BuildType
    {
        /// <summary>
        /// Represents the debug build.
        /// </summary>
        Debug = 0x01,
        /// <summary>
        /// Represents the release build.
        /// </summary>
        Release = 0x02,
        /// <summary>
        /// Represents an executable
        /// </summary>
        Executable = 0x04,
        /// <summary>
        /// Represents an external assembly
        /// </summary>
        Assembly = 0x08
    }

    /// <summary>
    /// Represents and contains some options for Expresso compiler.
    /// </summary>
    public class ExpressoCompilerOptions
    {
        /// <summary>
        /// Represents the resulting executable name.
        /// Note that this property doesn't include the file extension.
        /// </summary>
        /// <value>The name of the target.</value>
        public string ExecutableName{
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

