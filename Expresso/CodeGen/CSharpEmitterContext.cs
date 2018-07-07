using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Expresso.Ast;


namespace Expresso.CodeGen
{
    public enum OperationType
    {
        Load,
        Set,
        None
    }

    /// <summary>
    /// Represents the current context for <see cref="Expresso.CodeGen.CodeGenerator"/>.
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
        public WrappedTypeBuilder LazyTypeBuilder{
            get; set;
        }

        /// <summary>
        /// Current interface type builder.
        /// It can be null if we are not constructing an interface.
        /// </summary>
        public TypeBuilder InterfaceTypeBuilder{
            get; set;
        }

        /// <summary>
        /// Contains method info in which we have any interest.
        /// Setting it null may tell child nodes to fill in this property.
        /// </summary>
        public MethodInfo Method{
            get; set;
        }

        /// <summary>
        /// This flag indicates whether we are interested in methods or not.
        /// </summary>
        public bool RequestMethod{
            get; set;
        }

        /// <summary>
        /// Contains field info in which we have any interest.
        /// Like the <see cref="Method"/> property,
        /// setting it null indicates that you want to know which field we should be looking at.
        /// </summary>
        public MemberInfo PropertyOrField{
            get; set;
        }

        /// <summary>
        /// This flag indicates whether we are interested in fields or not.
        /// </summary>
        public bool RequestPropertyOrField{
            get; set;
        }

        /// <summary>
        /// Contains constructor information that we have any interest in.
        /// Setting it null may tell child nodes to fill this property.
        /// </summary>
        /// <value>The constructor.</value>
        public ConstructorInfo Constructor{
            get; set;
        }

        /// <summary>
        /// The type that is concerned to the current expression.
        /// </summary>
        public Type TargetType{
            get; set;
        }

        /// <summary>
        /// This flag indicates whether we are interested in types or not.
        /// </summary>
        public bool RequestType{
            get; set;
        }

        /// <summary>
        /// Currently targeted <see cref="LocalBuilder"/>.
        /// </summary>
        /// <value>The target local builder.</value>
        public LocalBuilder TargetLocalBuilder{
            get; set;
        }

        /// <summary>
        /// Indicates whether to set or load a local variable or an argument.
        /// </summary>
        public OperationType OperationTypeOnIdentifier{
            get; set;
        }

        /// <summary>
        /// Current context expression. That is, a C# expression we are constructing.
        /// </summary>
        public System.Linq.Expressions.Expression ContextExpression{
            get; set;
        }

        /// <summary>
        /// Represents the current context block.
        /// </summary>
        public System.Linq.Expressions.BlockExpression ContextBlock{
            get; set;
        }

        /// <summary>
        /// Current context ast node.
        /// </summary>
        public AstNode ContextAst{
            get; set;
        }

        /// <summary>
        /// The current context closure block.
        /// It is null if we are not inside a closure literal expression.
        /// </summary>
        /// <value>The context closure literal.</value>
        public ClosureLiteralExpression ContextClosureLiteral{
            get; set;
        }

        /// <summary>
        /// The current context closure type. Used to retrieve the lifted identifiers as fields.
        /// It is null if we are not inside a closure literal expression.
        /// </summary>
        /// <value>The context closure literal.</value>
        public Type ContextClosureType{
            get; set;
        }

        /// <summary>
        /// It will be set to a ParameterExpression that represents the temporary variable.
        /// </summary>
        public System.Linq.Expressions.ParameterExpression TemporaryVariable{
            get; set;
        }

        /// <summary>
        /// It will be set to an Expression that represents the temporary expression.
        /// </summary>
        /// <value>The temporary expression.</value>
        public System.Linq.Expressions.Expression TemporaryExpression{
            get; set;
        }

        /// <summary>
        /// Represents the local variables that the current expression refers to.
        /// </summary>
        /// <value>The additional parameters.</value>
        public List<LocalBuilder> Parameters{
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

        /// <summary>
        /// It will be set to a <see cref="System.Linq.Expressions.CatchBlock"/>.
        /// </summary>
        /// <value>The catch block.</value>
        public System.Linq.Expressions.CatchBlock CatchBlock{
            get; set;
        }

        /// <summary>
        /// The number of the current external module being inspected.
        /// </summary>
        /// <value>The current module count.</value>
        public int CurrentModuleCount{
            get; set;
        }

        /// <summary>
        /// Represents a <see cref="Type"/> instance that represents an external module type. 
        /// </summary>
        /// <value>The type of the external module.</value>
        public Type ExternalModuleType{
            get; set;
        }

        /// <summary>
        /// Represents the types of the arugments of a method.
        /// Used to resolve which method overload to call.
        /// </summary>
        /// <value>The argument types.</value>
        public Type[] ArgumentTypes{
            get; set;
        }

        /// <summary>
        /// Represents the currently focused custom attribute set method.
        /// </summary>
        /// <value>The custom attribute setter.</value>
        public Action<CustomAttributeBuilder> CustomAttributeSetter{
            get; set;
        }

        /// <summary>
        /// The target that we will apply the attribute.
        /// </summary>
        /// <value>The attribute target.</value>
        public AttributeTargets AttributeTarget{
            get; set;
        }

        /// <summary>
        /// Represents the current parameter index.
        /// </summary>
        /// <value>The index of the parameter.</value>
        public int ParameterIndex{
            get; set;
        }

        /// <summary>
        /// Indicates that we expect a reference at this point.
        /// </summary>
        /// <value><c>true</c> if expects reference; otherwise, <c>false</c>.</value>
        public bool ExpectsReference{
            get; set;
        }
    }
}

