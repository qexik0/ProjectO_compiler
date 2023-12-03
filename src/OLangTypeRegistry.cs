using System.Text;
using LLVMSharp.Interop;
using OCompiler.nodes;

namespace OCompiler.Codegen;

public class OLangSymbol
{
    public required string Class { get; set; }
    public LLVMTypeRef TypeRef { get; set; }
    public LLVMValueRef ValueRef { get; set; }
}

public class OLangParameter
{
    public required string Identifier { get; set; }
    public required string Class {get; set;}
}

public class OLangMethod
{
    public required string Name { get; set; }
    public List<OLangParameter> Parameters { get; set; } = new();
    public required string ReturnType { get; set; }
    public LLVMTypeRef FunctionType { get; set; }
    public LLVMValueRef FunctionRef { get; set; }
}

public class OLangClass
{
    public required string Identifier;
    public List<OLangMethod> Methods { get; set; } = new();
    public List<OLangParameter> Fields { get; set; } = new();
    public string? InheritedFrom { get; set; }
    public LLVMTypeRef ClassType { get; set;} 
}

public unsafe static class OLangTypeRegistry
{
    public static List<OLangClass> Types = new();

    public static void Init(in LLVMModuleRef module)
    {
        Types.Add(new () {Identifier = "AnyValue"});
        Types.Add(new () {Identifier = "AnyRef"});
        
        Types.Add(new() {Identifier = "Integer", InheritedFrom = "AnyValue", ClassType = LLVM.Int32TypeInContext(module.Context)});
        Types.Add(new() {Identifier = "Real", InheritedFrom = "AnyValue", ClassType = LLVM.DoubleTypeInContext(module.Context)});
        Types.Add(new() {Identifier = "Boolean", InheritedFrom = "AnyValue", ClassType = LLVM.Int1TypeInContext(module.Context)});
        Types.Add(new() {Identifier = "", ClassType = LLVM.VoidTypeInContext(module.Context)});

        // TODO: add std
    }

    public static OLangClass GetClass(string identifier)
    {
        return Types.Where(x => x.Identifier == identifier).First();
    }

    public static OLangParameter GetClassField(string currentClass, string variableName)
    {
        return GetClass(currentClass).Fields.Where(x => variableName == x.Identifier).First();
    }

    public static OLangMethod GetClassMethod(string currentClass, string methodName, List<string> parameters)
    {
        foreach (var method in GetClass(currentClass).Methods)
        {
            if (method.Name == methodName && method.Parameters.Count == parameters.Count)
            {
                bool good = true;
                for (int i = 0; i < parameters.Count; i++)
                {
                    if (parameters[i] != method.Parameters[i].Class)
                    {
                        good = false;
                        break;
                    }
                }
                if (good)
                {
                    return method;
                }
            }
        }
        throw new KeyNotFoundException("Method not found");
    }

    public static string FieldExpressionType(string currentClass, Expression expression)
    {
        var currentType = expression.PrimaryOrConstructorCall switch
        {
            Primary primary => primary.Node switch
            {
                IntegerLiteral => "Integer",
                RealLiteral => "Real",
                BooleanLiteral => "Boolean",
                ClassName className => GetClassField(currentClass, className.ClassIdentifier.Name).Class,
                _ => throw new ApplicationException($"Could not derive type for the expression {expression}")
            },
            ConstructorCall constructorCall => constructorCall.ConstructorClassName.ClassIdentifier.Name,
            _ => throw new ApplicationException($"Could not derive type for the expression {expression}")
        };
        foreach (var (identifier, arguments) in expression.Calls)
        {
            List<string> args = new();
            if (arguments != null)
            {
                foreach (var parameter in arguments.Expressions)
                {
                    var type = FieldExpressionType(currentClass, parameter);
                    args.Add(type);
                }
            }
            try
            {
                var method = GetClassMethod(currentClass, identifier.Name, args);
                currentType = method.ReturnType;
            }
            catch (Exception)
            {
                var field = GetClassField(currentClass, identifier.Name);
                currentType = field.Class;
            }
        }
        return currentType;
    }

    public static void CreateLLVMType(in LLVMModuleRef module, string className)
    {
        var classType = module.Context.CreateNamedStruct(className);
        List<LLVMTypeRef> memberTypes = new ();
        foreach (var field in GetClass(className).Fields)
        {
            memberTypes.Add(GetClass(field.Class).ClassType);
        }
        classType.StructSetBody(memberTypes.ToArray(), false);
        GetClass(className).ClassType = classType;
    }

    internal static void CreateLLVMConstructor(in LLVMModuleRef module, string className, OLangMethod method)
    {
        var mangledName = $"{className}%{string.Join(",", method.Parameters.Select(x => x.Class))}%";
        var returnType = GetClass(method.ReturnType).ClassType;
        var paramTypes = new List<LLVMTypeRef>() {LLVM.PointerTypeInContext(module.Context, 0)};
        foreach (var parameter in method.Parameters)
        {
            paramTypes.Add(GetClass(parameter.Class).ClassType);
        }
        fixed (LLVMTypeRef* paramsPtr = paramTypes.ToArray())
        {
            method.FunctionType = LLVM.FunctionType(returnType, (LLVMOpaqueType**) paramsPtr, (uint) paramTypes.Count, 0);
        }
        method.FunctionRef = module.AddFunction(mangledName, method.FunctionType);
    }

    internal static void CreateLLVMMethod(in LLVMModuleRef module, string className, OLangMethod method)
    {
        var mangledName = $"{className}.{method.Name}%{string.Join(",", method.Parameters.Select(x => x.Class))}%";
        var returnType = GetClass(method.ReturnType).ClassType;
        var paramTypes = new List<LLVMTypeRef>() {LLVM.PointerTypeInContext(module.Context, 0)};
        foreach (var parameter in method.Parameters)
        {
            paramTypes.Add(GetClass(parameter.Class).ClassType);
        }
        fixed (LLVMTypeRef* paramsPtr = paramTypes.ToArray())
        {
            method.FunctionType = LLVM.FunctionType(returnType, (LLVMOpaqueType**) paramsPtr, (uint) paramTypes.Count, 0);
        }
        method.FunctionRef = module.AddFunction(mangledName, method.FunctionType);
    }
}