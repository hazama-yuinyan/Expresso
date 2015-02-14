using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Expresso.Ast;


namespace Expresso.CodeGen
{
    /// <summary>
    /// Represents the current context for <see cref="Expresso.CodeGen.CSharpEmitter"/>.
    /// </summary>
    public class CSharpEmitterContext
    {
        public AssemblyBuilder AssemblyBuilder{
            get; set;
        }

        public ModuleBuilder ModuleBuilder{
            get; set;
        }

        public TypeBuilder TypeBuilder{
            get; set;
        }

        public MethodInfo Method{
            get; set;
        }

        public MemberInfo Member{
            get; set;
        }

        public FieldInfo Field{
            get; set;
        }

        public ConstructorInfo Constructor{
            get; set;
        }

        public ConstructorBuilder ConstructorBuilder{
            get; set;
        }

        /// <summary>
        /// In type declarations, this property represents the type being declared.
        /// </summary>
        public TypeDeclaration DeclaringType{
            get; set;
        }

        public Assembly Assembly{
            get; set;
        }

        public Module Module{
            get; set;
        }

        /// <summary>
        /// The type that is concerned to the current expression.
        /// </summary>
        public Type TargetType{
            get; set;
        }

        /// <summary>
        /// Current context expression. That is, a C# expression we are constructing.
        /// </summary>
        public System.Linq.Expressions.Expression ContextExpression{
            get; set;
        }

        /// <summary>
        /// It will be set a parameter expression that represents the temporary variable
        /// </summary>
        public System.Linq.Expressions.ParameterExpression TemporaryVariable{
            get; set;
        }

        /// <summary>
        /// Additional temporary expressions.
        /// Used in expressions that need complicated transformation.
        /// Usually set to null. Not being null indicates that the child nodes should add their results
        /// to this property(usually parameters).
        /// Its element type is object because some expression types aren't derived from
        /// <see cref="System.Linq.Expressions.Expression"/>.
        /// </summary>
        public List<object> Additionals{
            get; set;
        }
    }
}

