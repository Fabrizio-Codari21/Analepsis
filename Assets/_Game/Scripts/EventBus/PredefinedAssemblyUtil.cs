using System;
using System.Collections.Generic;
using System.Reflection;


public static class PredefinedAssemblyUtil
{
    enum AssemblyType
    {
        AssemblyCSharp,
        AssemblyCSharpEditor,
        AssemblyCSharpEditorFirstPass,
        AssemblyCSharpFirstPass
    }

    static AssemblyType? GetAssemblyType(string assemblyName)
    {
        return assemblyName switch
        {
            "Assembly-CSharp" => AssemblyType.AssemblyCSharp,
            "Assembly-CSharp-Editor" => AssemblyType.AssemblyCSharpEditor,
            "Assembly-CSharp-Editor-firstpass" => AssemblyType.AssemblyCSharpEditorFirstPass,
            "Assembly-CSharp-firstpass" => AssemblyType.AssemblyCSharpFirstPass,
            _ => null
        };
    }
    static void AddTypesFromAssembly(Type[] assemblyTypes, Type interfaceType, ICollection<Type> results)
    {
        if (assemblyTypes == null) return;
        foreach (var type in assemblyTypes)
        {
            if (type != interfaceType && interfaceType.IsAssignableFrom(type)) {
                results.Add(type);
            }
        }
    }

    public static List<Type> GetTypes(Type interfaceType) {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        Dictionary<AssemblyType, Type[]> assemblyTypes = new Dictionary<AssemblyType, Type[]>();
        List<Type> types = new List<Type>();
        foreach (var t in assemblies)
        {
            AssemblyType? assemblyType = GetAssemblyType(t.GetName().Name);
            if (assemblyType != null) {
                assemblyTypes.Add((AssemblyType) assemblyType, t.GetTypes());
            }
        }
        
        assemblyTypes.TryGetValue(AssemblyType.AssemblyCSharp, out var assemblyCSharpTypes);
        AddTypesFromAssembly(assemblyCSharpTypes, interfaceType, types);

        assemblyTypes.TryGetValue(AssemblyType.AssemblyCSharpFirstPass, out var assemblyCSharpFirstPassTypes);
        AddTypesFromAssembly(assemblyCSharpFirstPassTypes, interfaceType, types);
        
        return types;
    }
}