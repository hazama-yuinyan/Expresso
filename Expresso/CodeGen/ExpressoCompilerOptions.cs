using System;
using ICSharpCode.NRefactory.TypeSystem.Implementation;

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
    public class ExpressoCompilerOptions : AbstractFreezable
    {
        string executable_name;

        /// <summary>
        /// Represents the resulting executable name.
        /// Note that this property doesn't include the file extension.
        /// </summary>
        /// <value>The name of the target.</value>
        public string ExecutableName{
            get{return executable_name;}
            set{
                FreezableHelper.ThrowIfFrozen(this);
                executable_name = value;
            }
        }

        BuildType build_type;

        /// <summary>
        /// Specifies the build type.
        /// In debug build, no optimizations will be performed.
        /// </summary>
        public BuildType BuildType{
            get{return build_type;}
            set{
                FreezableHelper.ThrowIfFrozen(this);
                build_type = value;
            }
        }

        string output_path;

        /// <summary>
        /// Specifies the output executable or assembly path.
        /// </summary>
        public string OutputPath{
            get{return output_path;}
            set{
                FreezableHelper.ThrowIfFrozen(this);
                output_path = value;
            }
        }
    }
}

