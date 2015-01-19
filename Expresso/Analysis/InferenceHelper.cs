using System;
using Expresso.Ast;
using Expresso.Compiler.Meta;


namespace Expresso.Ast.Analysis
{
    /// <summary>
    /// Contains logics for inference.
    /// Used during post-parse processing.
    /// </summary>
    public static class InferenceHelper
    {
        public static TypeAnnotation InferType(Expression inferenceTarget)
        {
            var mem_ref = inferenceTarget as MemberReference;
            if(mem_ref != null){
                return InferComplicatedType(mem_ref);
            }else{
                var ident = inferenceTarget as Identifier;
                if(ident != null)
                    return ident.Type;

                var seq_initializer = inferenceTarget as SequenceInitializer;
                if(seq_initializer != null)
                    return new TypeAnnotation(seq_initializer.ObjectType);

                var literal = inferenceTarget as LiteralExpression;
                if(literal != null)
                    return new TypeAnnotation(literal.Type);

                throw new ParserException("{0} : {1} -- Can not infer the type of the variable from the right-hand-side expression.",
                    inferenceTarget.StartLocation.Line, inferenceTarget.StartLocation.Column);
            }
        }

        public static TypeAnnotation InferTypeForForStatement(Expression target)
        {
            Identifier ident = target as Identifier;
            if(ident != null){
                if(!IsIterableType(ident.Type)){
                    throw new ParserException("{0} : {1} -- The right-hand-side of a for statement must be an expression yielding an iterable type.",
                        target.StartLocation.Line, target.StartLocation.Column);
                }

                //TODO: iterableの中身を参照して型推論をする
                throw new NotImplementedException();
            }

            var initializer = target as SequenceInitializer;
            if(initializer != null){
                foreach(var item in initializer.Items){
                    
                }
            }
        }

        static TypeAnnotation InferComplicatedType(MemberReference memRef)
        {
            MemberReference parent = null, mem_ref;
            Expression tmp = memRef;
            while((mem_ref = tmp as MemberReference) != null){
                parent = mem_ref;
                tmp = mem_ref.Subscription;
            }

            var parent_ident = (parent != null) ? parent.Target as Identifier : null;
            if(parent_ident != null){
                if(tmp is LiteralExpression || tmp is IntegerSequenceExpression){    //a.b or a[$n] where $n is an integer or a string
                    return parent_ident.Type;
                }else{
                    var method_call = parent.Subscription as CallExpression;
                    var method_ident = method_call.Target as Identifier;
                    throw new NotImplementedException();
                }
            }

            throw new ParserException("{0} : {1} -- Can not infer the type of the variable from the right-hand-side expression.",
                memRef.StartLocation.Line, memRef.StartLocation.Column);
        }

        static bool IsIterableType(TypeAnnotation type)
        {
            return type.ObjType == ObjectTypes.Dict || type.ObjType == ObjectTypes.List ||
                type.ObjType == ObjectTypes.Tuple;
        }
    }
}

