using System.Text;
using LLVMSharp.Interop;
using OCompiler.nodes;

namespace OCompiler.Codegen;

public class OLangClass
{
    public Dictionary<string, string> FieldTypes { get; set; } = new ();
    public Dictionary<string, string> MethodReturnTypes { get; set; } = new ();
}

public unsafe static class OLangTypeRegistry
{
    private static readonly Dictionary<string, OLangClass> typeVariables = new ();
    private static readonly Dictionary<string, string> inheritanceRelation = new ();
    private static readonly Dictionary<string, LLVMTypeRef> llvmTypes = new ();

    static OLangTypeRegistry()
    {
        // typeVariables["AnyVal"] = new ();
        // typeVariables["Integer"] = new ();
        // typeVariables["Real"] = new ();
        // typeVariables["Boolean"] = new ();
        // typeVariables["AnyRef"] = new ();

        llvmTypes["Integer"] = LLVM.Int32Type();
        llvmTypes["Real"] = LLVM.DoubleType();
        llvmTypes["Boolean"] = LLVM.Int1Type();

        inheritanceRelation.Add("Integer", "AnyVal");
        inheritanceRelation.Add("Real", "AnyVal");
        inheritanceRelation.Add("Boolean", "AnyVal");

    }

    // public static void AddClassField(string className, string fieldName, string fieldType)
    // {
    //     typeVariables[className].FieldTypes[fieldName] = fieldType;
    // }

    // public static void AddClassMethod(string className, string methodName, string returnType)
    // {
    //     typeVariables[className].MethodReturnTypes[methodName] = returnType;
    // }

    // public static void AddClass(string className, string baseClass = "AnyRef")
    // {
    //     typeVariables[className] = new ();
    //     inheritanceRelation[className] = baseClass;
    // }

    public static string ClassExpressionType(string curClass, Expression expression)
    {
        var currentType = expression.PrimaryOrConstructorCall switch
        {
            Primary primary => primary.Node switch
            {
                IntegerLiteral => "Integer",
                RealLiteral => "Real",
                BooleanLiteral => "Boolean",
                ClassName className => GetVariableType(curClass, className.ClassIdentifier.Name), // could only be identifier
                _ => throw new ApplicationException($"Could not derive type for the expression {expression}")
            },
            ConstructorCall constructorCall => MangleClassName(constructorCall.ConstructorClassName),
            _ => throw new ApplicationException($"Could not derive type for the expression {expression}")
        };
        foreach (var (identifier, arguments) in expression.Calls)
        {
            if (!typeVariables.TryGetValue(currentType, out var _))
            {
                throw new ApplicationException($"Type {currentType} was not defined earlier");
            }
            var returnType = typeVariables[currentType].MethodReturnTypes[MangleFunctionName(currentType, identifier, arguments)];
            currentType = returnType;
        }
        return currentType;
    }

    public static string MangleClassName(ClassName className)
    {
        StringBuilder sb = new();
        sb.Append(className.ClassIdentifier.Name);
        var genericName = className.GenericClassName;
        int genericNames = 0;
        while (genericName != null)
        {
            sb.Append($"#{genericName.ClassIdentifier.Name}");
            genericName = genericName.GenericClassName;
            ++genericNames;
        }
        while (genericNames > 0)
        {
            genericNames--;
            sb.Append('#');
        }
        return sb.ToString();
    }

    public static string MangleFunctionName(ConstructorCall constructorCall)
    {
        StringBuilder sb = new ();
        sb.Append(MangleClassName(constructorCall.ConstructorClassName));
        sb.Append("%");
        if (constructorCall.ConstructorArguments == null)
        {
            sb.Append("%");
            return sb.ToString();
        }
        List<string> mangledArgumentTypes = new ();
        foreach (var argument in constructorCall.ConstructorArguments.Expressions)
        {
            var argumentType = ClassExpressionType(MangleClassName(constructorCall.ConstructorClassName), argument);
            mangledArgumentTypes.Add(argumentType);
        }
        sb.Append(string.Join(',', mangledArgumentTypes));
        sb.Append('%');
        return sb.ToString();
    }

    public static string MangleFunctionName(string className, ConstructorDeclaration constructorDecl)
    {
        StringBuilder sb = new ();
        sb.Append(className);
        sb.Append("%");
        if (constructorDecl.ConstructorParameters == null)
        {
            sb.Append("%");
            return sb.ToString();
        }
        List<string> mangledArgumentTypes = new ();
        foreach (var argument in constructorDecl.ConstructorParameters.ParameterDeclarations)
        {
            var argumentType = MangleClassName(argument.ParameterClassName);
            mangledArgumentTypes.Add(argumentType);
        }
        sb.Append(string.Join(',', mangledArgumentTypes));
        sb.Append('%');
        return sb.ToString();
    }

    public static string MangleFunctionName(string className, Identifier identifier, Arguments? arguments)
    {
        StringBuilder sb = new ();
        sb.Append($"{className}.{identifier}");
        sb.Append("%");
        if (arguments == null)
        {
            sb.Append("%");
            return sb.ToString();
        }
        List<string> mangledArgumentTypes = new ();
        foreach (var argument in arguments.Expressions)
        {
            var argumentType = ClassExpressionType(className, argument);
            mangledArgumentTypes.Add(argumentType);
        }
        sb.Append(string.Join(',', mangledArgumentTypes));
        sb.Append('%');
        return sb.ToString();
    }

    // public static string MangleFunctionName(string className, MethodDeclaration methodDeclaration)
    // {
    //     StringBuilder sb = new ();
    //     sb.Append($"{className}.{methodDeclaration.MethodIdentifier.Name}");
    //     sb.Append("%");
    //     if (methodDeclaration.MethodParameters == null)
    //     {
    //         sb.Append("%");
    //         return sb.ToString();
    //     }
    //     List<string> mangledArgumentTypes = new ();
    //     foreach (var argument in methodDeclaration.MethodParameters.ParameterDeclarations)
    //     {
    //         var argumentType = MangleClassName(argument.ParameterClassName);
    //         mangledArgumentTypes.Add(argumentType);
    //     }
    //     sb.Append(string.Join(',', mangledArgumentTypes));
    //     sb.Append('%');
    //     return sb.ToString();
    // }

    public static string GetVariableType(string className, string identifier)
    {
        return typeVariables[className].FieldTypes[identifier];
    }

    // public static LLVMTypeRef GetLLVMClassType(in LLVMModuleRef module, in LLVMBuilderRef builder, string className)
    // {
    //     if (llvmTypes.ContainsKey(className))
    //     {
    //         return llvmTypes[className];
    //     }
    //     LLVMTypeRef classType = module.Context.CreateNamedStruct(className);
    //     List<LLVMTypeRef> memberTypes = new List<LLVMTypeRef>();
    //     foreach (var field in typeVariables[className].FieldTypes.Keys)
    //     {
    //         memberTypes.Add(GetLLVMClassType(module, builder, typeVariables[className].FieldTypes[field]));
    //     }
    //     classType.StructSetBody(memberTypes.ToArray(), false);
    //     llvmTypes[className] = classType;
    //     return classType;
    // }

    // public static unsafe  LLVMTypeRef GetLLVMMethodType(string className, MethodDeclaration methodDeclaration)
    // {
    //     var methodName = MangleFunctionName(className, methodDeclaration);
    //     if (llvmTypes.ContainsKey(methodName))
    //     {
    //         return llvmTypes[methodName];
    //     }
    //     var olangRetType = typeVariables[className].MethodReturnTypes[methodName];
    //     LLVMTypeRef returnType;
    //     if (olangRetType.EndsWith("*"))
    //     {
    //         olangRetType = olangRetType.Substring(0, olangRetType.Length - 1);
    //         returnType = LLVM.PointerType(llvmTypes[olangRetType], 0);
    //     }
    //     else
    //     {
    //         returnType = llvmTypes[olangRetType];
    //     }
    //     var paramTypes = new List<LLVMTypeRef>
    //     {
    //         LLVM.PointerType(llvmTypes[className], 0)
    //     };
    //     if (methodDeclaration.MethodParameters != null)
    //     {
    //         foreach (var parameter in methodDeclaration.MethodParameters.ParameterDeclarations)
    //         {
    //             LLVMTypeRef parameterType = llvmTypes[MangleClassName(parameter.ParameterClassName)];
    //             paramTypes.Add(parameterType);
    //         }
    //     }
    //     fixed (LLVMTypeRef* paramsPtr = paramTypes.ToArray())
    //     {
    //         var methodType = LLVM.FunctionType(returnType, (LLVMOpaqueType**) paramsPtr, (uint) paramTypes.Count, 0);
    //         llvmTypes[methodName] = methodType;
    //         return methodType;
    //     }
    // }

    // public static unsafe  LLVMTypeRef GetLLVMMethodType(string className, ConstructorDeclaration constructorDeclaration)
    // {
    //     var methodName = MangleFunctionName(className, constructorDeclaration);
    //     if (llvmTypes.ContainsKey(methodName))
    //     {
    //         return llvmTypes[methodName];
    //     }
    //     var olangRetType = typeVariables[className].MethodReturnTypes[methodName];
    //     LLVMTypeRef returnType;
    //     if (olangRetType.EndsWith("*"))
    //     {
    //         olangRetType = olangRetType.Substring(0, olangRetType.Length - 1);
    //         returnType = LLVM.PointerType(llvmTypes[olangRetType], 0);
    //     }
    //     else
    //     {
    //         returnType = llvmTypes[olangRetType];
    //     }
    //     var paramTypes = new List<LLVMTypeRef>
    //     {
    //         LLVM.PointerType(llvmTypes[className], 0)
    //     };
    //     if (constructorDeclaration.ConstructorParameters != null)
    //     {
    //         foreach (var parameter in methodDeclaration.MethodParameters.ParameterDeclarations)
    //         {
    //             LLVMTypeRef parameterType = llvmTypes[MangleClassName(parameter.ParameterClassName)];
    //             paramTypes.Add(parameterType);
    //         }
    //     }
    //     fixed (LLVMTypeRef* paramsPtr = paramTypes.ToArray())
    //     {
    //         var methodType = LLVM.FunctionType(returnType, (LLVMOpaqueType**) paramsPtr, (uint) paramTypes.Count, 0);
    //         llvmTypes[methodName] = methodType;
    //         return methodType;
    //     }
    // }

    // public static void AddMethod(string className, MethodDeclaration methodDeclaration)
    // {
    //     var methodName = MangleFunctionName(className, methodDeclaration);
    //     if (methodDeclaration.ReturnTypeIdentifier == null)
    //     {
    //         AddClassMethod(className, methodName, "");
    //     }
    //     else
    //     {
    //         AddClassMethod(className, methodName, MangleClassName(methodDeclaration.ReturnTypeIdentifier));
    //     }
    // }

    // public static void AddMethod(string className, ConstructorDeclaration constructorDeclaration)
    // {
    //     var methodName = MangleFunctionName(className, constructorDeclaration);
    //     if (IsInheritedFrom(className, "AnyRef"))
    //     {
    //         AddClassMethod(className, methodName, "");
    //     }
    //     else
    //     {
    //         AddClassMethod(className, methodName, $"{className}*");
    //     }
    // }

    // public static bool IsInheritedFrom(string className, string baseClass)
    // {
    //     while (inheritanceRelation.TryGetValue(className, out var inheritedFrom))
    //     {
    //         if (inheritedFrom == baseClass)
    //         {
    //             return true;
    //         }
    //         className = inheritedFrom;
    //     }
    //     return false;
    // }
}