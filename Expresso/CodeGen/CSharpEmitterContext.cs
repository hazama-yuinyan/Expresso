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
        /// <summary>
        /// Current assembly builder that we are using.
        /// </summary>
        public AssemblyBuilder AssemblyBuilder{
            get; set;
        }

        /// <summary>
        /// Current module builder that we are using.
        /// </summary>
        public ModuleBuilder ModuleBuilder{
            get; set;
        }

        /// <summary>
        /// Current type builder.
        /// It will be null if we are not constructing a type declaration.
        /// </summary>
        public TypeBuilder TypeBuilder{
            get; set;
        }

        /// <summary>
        /// Contains method info in which we have any interest.
        /// Setting it null may tell child nodes to fill in this property.
        /// </summary>
        public MethodInfo Method{
            get; set;
        }

        public MemberInfo Member{
            get; set;
        }

        /// <summary>
        /// Contains field info in which we have any interest.
        /// Like the <see cref="Expresso.CodeGenCSharpEmitterContext.Method"/> property,
        /// setting it null indicates that you want to know which field we should be looking at.
        /// </summary>
        public FieldInfo Field{
            get; set;
        }

        public ConstructorInfo Constructor{
            get; set;
        }

        /// <summary>
        /// The constructor builder that we are currently constructing.
        /// </summary>
        public ConstructorBuilder ConstructorBuilder{
            get; set;
        }

        /// <summary>
        /// In type declarations, this property represents the type being declared.
        /// </summary>
        public TypeDeclaration DeclaringType{
            get; set;
        }

        /// <summary>
        /// The assembly object that we are currently constructing.
        /// </summary>
        public Assembly Assembly{
            get; set;
        }

        /// <summary>
        /// The module object that we are targeting at.
        /// </summary>
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
        /// It will be set to a parameter expression that represents the temporary variable.
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

