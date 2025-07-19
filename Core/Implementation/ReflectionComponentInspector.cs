using System.Reflection;
using ComponentInspector.Core.Interfaces;
using ComponentInspector.Models;
using ComponentInspector.Resources;

namespace ComponentInspector.Core.Implementation;

using SystemTypes = ApplicationConstants.SystemTypes;
using Modifiers = ApplicationConstants.Modifiers;

internal class ReflectionComponentInspector(ILogger logger) : IComponentInspector
{
    #region Fields

    private const BindingFlags MemberDiscoveryFlags = BindingFlags.Public | BindingFlags.NonPublic |
                                                      BindingFlags.Instance | BindingFlags.Static;
    private readonly Dictionary<Type, object> _instanceCache = new();
    private Assembly? _assembly;
    private Type[]? _types;
    
    #endregion // =================================================================================
    
    #region Implementation methods

    public void InspectAssembly(Assembly assembly)
    {
        _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        _instanceCache.Clear();
        _types = _assembly.GetTypes();
        logger.Write(FormatTemplates.FoundTypesAmount.Format(_types.Length));
    }
    
    public void ListTypes()
    { 
        EnsureAssemblyLoaded();
        logger.Write(UIStrings.TypesSection);
        if (_types!.Length == 0) 
        {
            logger.Write(UIStrings.NonePresent);
            return;
        }
        foreach (var type in _types!)
        {
            logger.Write(FormatTemplates.TypeHeader.Format(type.FullName));
            // Type details
            var baseType = type.BaseType?.FullName ?? "None";
            var isStatic = type is { IsAbstract: true, IsSealed: true };
            var isInternal = type.IsNotPublic || type is { IsPublic: false, IsNestedPublic: false };
            logger.Write(FormatTemplates.TypeDetails.Format(
                type.IsPublic || type.IsNestedPublic,
                isInternal,
                type.IsClass && !IsDelegate(type),
                type.IsInterface,
                type.IsAbstract && !isStatic,
                type.IsSealed && !isStatic,
                isStatic,
                type.IsValueType,
                type.IsEnum,
                baseType
            ));
            // Interfaces
            var interfaces = type.GetInterfaces();
            if (interfaces.Length > 0)
            {
                logger.Write(FormatTemplates.InterfacesHeader);
                foreach (var i in interfaces)
                {
                    logger.Write("[tab][tab][tab]");
                    logger.Write(FormatTemplates.InterfaceSyntax.Format(i.FullName));
                }
            }
            // Fields
            var fields = type.GetFields(MemberDiscoveryFlags);
            if (fields.Length > 0)
            {
                logger.Write(FormatTemplates.FieldsHeader);
                foreach (var field in fields)
                {
                    logger.Write("[tab][tab][tab]");
                    logger.Write(FormatTemplates.FieldSyntax.Format(
                        GetMemberModifiers(field),
                        field.FieldType.Name,
                        field.Name
                        ));
                }
            }
            // Properties
            var properties = type.GetProperties(MemberDiscoveryFlags);
            if (properties.Length > 0) 
            {
                logger.Write(FormatTemplates.PropertiesHeader);
                foreach (var property in properties)
                {
                    var accessor = property.GetGetMethod(true) ?? property.GetSetMethod(true);
                    logger.Write("[tab][tab][tab]");
                    logger.Write(FormatTemplates.PropertySyntax.Format(
                        GetMemberModifiers(accessor),
                        property.PropertyType.Name,
                        property.Name,
                        property.CanRead ? " get" : "",
                        property is { CanRead: true, CanWrite: true } ? ";" : "",
                        property.CanWrite ? " set" : ""
                        ));
                }
            }
            // Constructors
            var constructors = type.GetConstructors(MemberDiscoveryFlags);
            if (constructors.Length > 0)
            {
                logger.Write(FormatTemplates.ConstructorsHeader); 
                foreach (var ctor in constructors)
                { 
                    logger.Write("[tab][tab][tab]");
                    logger.Write(FormatTemplates.ConstructorSyntax.Format(
                        GetMemberModifiers(ctor),
                        type.Name,
                        FormatParameters(ctor)
                        ));
                }
            }
            // Methods
            var methods = type.GetMethods(MemberDiscoveryFlags);
            if (methods.Length > 0)
            {
                logger.Write(FormatTemplates.MethodsHeader); 
                foreach (var method in methods)
                {
                    var isDeclared = method.DeclaringType == type;
                    var prefix = isDeclared ? "" : "[verbose](inherited) [/]";
                    logger.Write("[tab][tab][tab]" + prefix);
                    logger.Write(FormatTemplates.ShortMethodSyntax.Format(
                        GetMemberModifiers(method),
                        method.ReturnType.Name,
                        method.Name,
                        FormatParameters(method)
                        ));
                }
            }
        }
    }
    
    public void ListFields() => ListMembers(
        UIStrings.FieldsSection, 
        type => type.GetFields(MemberDiscoveryFlags),
        (type, field) => 
        {
            var isDeclared = field.DeclaringType == type;
            var prefix = isDeclared ? "" : "[verbose](inherited) [/]";
            return prefix + FormatTemplates.FieldSyntax.Format(
                GetMemberModifiers(field), 
                field.FieldType.Name, 
                field.Name
                );
        }
        );
    
    public void ListProperties() => ListMembers(
        UIStrings.PropertiesSection,
        type => type.GetProperties(MemberDiscoveryFlags),
        (_, member) => FormatTemplates.PropertySyntax.Format(
            GetMemberModifiers(member.GetGetMethod(true) ?? member.GetSetMethod(true)),
            member.PropertyType.Name,
            member.Name,
            member.CanRead ? " get" : "",
            member is { CanRead: true, CanWrite: true } ? ";" : "", 
            member.CanWrite ? " set" : ""
            )
        );
    
    public void ListMethods() => ListMembers(
        UIStrings.MethodsSection, 
        type => type.GetMethods(MemberDiscoveryFlags),
        (type, member) => 
        {
            var isDeclared = member.DeclaringType == type;
            var prefix = isDeclared ? "" : "[verbose](inherited) [/]";
            return prefix + FormatTemplates.MethodSyntax.Format(
                GetMemberModifiers(member),
                member.ReturnType.Name,
                type.Name, 
                member.Name, 
                FormatParameters(member)
                );
        },
        groupByType: false
        );
    
    public void DumpAll()
    {
        EnsureAssemblyLoaded();
        logger.Write(UIStrings.DumpSection);
        var namespaceGroups = _types!
            .Where(t => !t.IsNested)
            .GroupBy(t => t.Namespace ?? ApplicationConstants.DefaultNamespace)
            .OrderBy(g => g.Key);
        foreach (var namespaceGroup in namespaceGroups)
        {
            logger.Write("[br][tab]");
            logger.Write(FormatTemplates.NamespaceSyntax.Format(namespaceGroup.Key));
            logger.Write("[tab]{");
            foreach (var type in namespaceGroup)
            {
                DumpType(type, 2);
            }
            logger.Write("[tab]}[br]");
        }
    }
    
    public void InvokeMethod(MethodInvocation invocation)
    {
        EnsureAssemblyLoaded();
        var lastDot = invocation.FullMethodName.LastIndexOf('.');
        if (lastDot == -1)
        {
            throw new ArgumentException(UIStrings.ErrorMessages.InvalidMethodFormat);
        }
        var typeName = invocation.FullMethodName[..lastDot];
        var methodName = invocation.FullMethodName[(lastDot + 1)..];
        var type = _types!.FirstOrDefault(t => t.FullName == typeName);
        if (type == null)
        {
            switch (typeName)
            {
                case SystemTypes.Math:
                    type = typeof(Math);
                    break;
                case SystemTypes.Console or SystemTypes.SystemConsole:
                    type = typeof(Console);
                    break;
                default:
                    type = Type.GetType(typeName + SystemTypes.MsCorLib) ??
                           Type.GetType(typeName + SystemTypes.SystemRuntime);
                    if (type == null && !typeName.StartsWith(SystemTypes.SystemPrefix))
                    {
                        var mscorlibQualifier = SystemTypes.SystemPrefix + typeName + ", " + 
                                                SystemTypes.MsCorLib;
                        var runtimeQualifier = SystemTypes.SystemPrefix + typeName + ", " + 
                                               SystemTypes.SystemRuntime;
                        type = Type.GetType(mscorlibQualifier) ?? Type.GetType(runtimeQualifier);
                    }
                    if (type == null)
                    {
                        throw new TypeLoadException(
                            FormatTemplates.ErrorMessages.TypeNotFound.Format(typeName));
                    }
                    break;
            }
        }
        var methods = type
            .GetMethods(MemberDiscoveryFlags)
            .Where(m => m.Name == methodName)
            .ToArray();
        if (methods.Length == 0)
        {
            throw new MethodAccessException(
                FormatTemplates.ErrorMessages.MethodNotFound.Format(methodName, typeName));
        }
        var argsLen = invocation.Arguments.Length;
        MethodInfo? method;
        if (methods.Length == 1)
        {
            method = methods[0];
        }
        else
        {
            method = methods
                .Where(m => m.GetParameters().Length == argsLen)
                .OrderBy(m =>
                {
                    return argsLen switch
                    {
                        1 when m.GetParameters()[0].ParameterType == typeof(string) => 0,
                        1 when m.GetParameters()[0].ParameterType == typeof(object) => 1,
                        _ => 2
                    };
                })
                .FirstOrDefault();
            if (method == null)
            {
                throw new MethodAccessException(
                    FormatTemplates.ErrorMessages.NoOverloadFound.Format(methodName, argsLen));
            }
        }
        object? instance = null;
        var wasCached = false;
        if (!method.IsStatic)
        {
            if (type.IsAbstract)
            {
                throw new MethodAccessException(
                    FormatTemplates.ErrorMessages.CannotCreateAbstract.Format(typeName));
            }
            if (!_instanceCache.TryGetValue(type, out instance))
            {
                instance = Activator.CreateInstance(type);
                _instanceCache[type] = instance!;
            }
            else
            {
                wasCached = true;
            }
        }
        var parameters = method.GetParameters();
        var convertedArgs = new object?[argsLen];
        var formattedArgs = new List<string>();
        for (var i = 0; i < argsLen; i++)
        {
            var param = parameters[i];
            var paramType = parameters[i].ParameterType;
            var paramName = param.Name ?? $"{ApplicationConstants.DefaultArgPrefix}{i}";
            var arg = invocation.Arguments[i];
            try
            {
                if (paramType == typeof(string))
                {
                    convertedArgs[i] = arg;
                    continue;
                }
                var underlyingType = Nullable.GetUnderlyingType(paramType);
                if (underlyingType != null)
                {
                    if (string.IsNullOrEmpty(arg) || arg.Equals(
                            ApplicationConstants.NullString, StringComparison.OrdinalIgnoreCase))
                    {
                        convertedArgs[i] = null;
                    }
                    else
                    {
                        convertedArgs[i] = Convert.ChangeType(arg, underlyingType);
                    }
                    continue;
                }
                convertedArgs[i] = Convert.ChangeType(arg, paramType);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(FormatTemplates.ErrorMessages.ConversionFail.Format(
                    arg, paramType.Name, ex.Message));
            }
            var formattedValue = paramType.Name switch
            {
                SystemTypes.String => FormatTemplates.StringSyntax.Format(arg), 
                SystemTypes.Char => FormatTemplates.CharSyntax.Format(arg),
                SystemTypes.Boolean => arg.ToLower(),
                _ => arg
            };
            formattedArgs.Add(
                FormatTemplates.ParamHintSyntax.Format(paramType.Name, paramName, formattedValue));
        }
        var prefix = instance == null ? "" : wasCached ? UIStrings.CachedHint : UIStrings.NewHint;
        var suffix = instance == null ? "" : wasCached ? "" : "()";
        var typeFormat = FormatTemplates.TypeSyntax.Format(prefix, typeName, suffix);
        logger.Write(FormatTemplates.InvocationDetails.Format(
            typeFormat, methodName, string.Join(", ", formattedArgs)));
        var result = method.Invoke(instance, convertedArgs);
        if (method.ReturnType == typeof(void))
        {
            result = UIStrings.NoResult;
        }
        logger.Write(FormatTemplates.ResultDetails.Format(result));
    }
    
    #endregion // =================================================================================
    
    #region Helper methods
    
    private void EnsureAssemblyLoaded()
    { 
        if (_assembly is null || _types is null) 
        { 
            throw new InvalidOperationException(UIStrings.ErrorMessages.NoAssemblyLoaded); 
        }
    }
    
    private void ListMembers<TMember>(
        string header,
        Func<Type, TMember[]> memberSelector,
        Func<Type, TMember, string> memberFormatter,
        bool groupByType = true) 
        where TMember : MemberInfo
    {
        EnsureAssemblyLoaded();
        logger.Write(header);
        var displayedMembers = false;
        foreach (var type in _types!)
        {
            var members = memberSelector(type); 
            if (members.Length <= 0) continue;
            if (groupByType)
            {
                logger.Write(FormatTemplates.TypeHeader.Format(type.FullName)); 
            }
            foreach (var member in members)
            {
                logger.Write(groupByType ? "[tab][tab]" : "[tab]");
                logger.Write(memberFormatter(type, member));
            }
            displayedMembers = true;
        }
        if (!displayedMembers) 
        {
            logger.Write(UIStrings.NonePresent);
        }
    }
    
    private void DumpType(Type type, int indentLevel)
    {
        logger.Write("[br]");
        var indent = string.Concat(Enumerable.Repeat("[tab]", indentLevel));
        var isDelegate = IsDelegate(type);
        var delegateSyntax = "";
        if (isDelegate)
        {
            var invokeMethod = type.GetMethod("Invoke");
            if (invokeMethod != null)
            {
                delegateSyntax = $"[pink]{invokeMethod.ReturnType.Name}[/] " +
                                 $"[sky]{type.Name}[/]({FormatParameters(invokeMethod)});";
            }
        }
        var name = type switch 
        {
            { IsInterface: true } => $"[aqua]{type.Name}[/]",
            { IsEnum: true } or { IsValueType: true } => $"[pink]{type.Name}[/]",
            _ when isDelegate => delegateSyntax,
            _ => $"[purple]{type.Name}[/]"
        };
        logger.Write($"{indent}[blue]{GetMemberModifiers(type)}[/] {name}");
        if (isDelegate)
        {
            logger.Write("[br]");
            return;
        }
        var baseTypes = new List<string>();
        if (type.BaseType != null && type.BaseType != typeof(object) && 
            type.BaseType != typeof(ValueType) && type.BaseType != typeof(Enum))
        {
            baseTypes.Add($"[pink]{type.BaseType.Name}[/]");
        }
        var interfaces = type.GetInterfaces();
        if (interfaces.Length > 0)
        {
            baseTypes.AddRange(
                interfaces
                    .Except(interfaces.SelectMany(i => i.GetInterfaces()))
                    .Select(i => $"[aqua]{i.Name}[/]")
            );
        }
        if (baseTypes.Count > 0)
        {
            logger.Write(" : " + string.Join(", ", baseTypes));
        }
        var enumValues = type.IsEnum ? Enum.GetNames(type) : null;
        var fields = type.GetFields(MemberDiscoveryFlags);
        var properties = type.GetProperties(MemberDiscoveryFlags);
        var constructors = type.GetConstructors(MemberDiscoveryFlags);
        var methods = type.GetMethods(MemberDiscoveryFlags);
        var nestedTypes = type.GetNestedTypes(MemberDiscoveryFlags);
        var hasContent = enumValues?.Length > 0 || fields.Length > 0 || properties.Length > 0 ||
                         constructors.Length > 0 || methods.Length > 0 || nestedTypes.Length > 0;
        if (!hasContent)
        {
            logger.Write(UIStrings.EmptyTypeBody);
            return;
        }
        logger.Write($"[br]{indent}{{");
        if (type.IsEnum && enumValues!.Length > 0)
        {
            for (var i = 0; i < enumValues.Length; i++)
            {
                logger.Write($"[br]{indent}[tab][pink]{enumValues[i]}[/]");
                if (i < enumValues.Length - 1) logger.Write(", ");
            }
            logger.Write("[br]");
        }
        foreach (var ctor in constructors)
        {
            logger.Write($"[br]{indent}[tab][blue]{GetMemberModifiers(ctor)}[/] ");
            logger.Write($"[purple]{type.Name}[/]({FormatParameters(ctor)});[br]");
        }
        if (fields.Length > 0)
        {
            logger.Write("[br]");
        }
        foreach (var field in fields)
        {
            logger.Write($"{indent}[tab]");
            logger.Write(FormatTemplates.FieldSyntax.Format(
                GetMemberModifiers(field),
                field.FieldType.Name,
                field.Name
                ));
        }
        if (properties.Length > 0)
        {
            logger.Write("[br]");
        }
        foreach (var property in properties)
        {
            var accessor = property.GetGetMethod(true) ?? property.GetSetMethod(true);
            logger.Write($"{indent}[tab]");
            logger.Write(FormatTemplates.PropertySyntax.Format(
                GetMemberModifiers(accessor),
                property.PropertyType.Name,
                property.Name,
                property.CanRead ? " get" : "",
                property is { CanRead: true, CanWrite: true } ? ";" : "",
                property.CanWrite ? " set" : ""
                ));
        }
        if (methods.Length > 0)
        {
            logger.Write("[br]");
        }
        foreach (var method in methods)
        {
            var isDeclared = method.DeclaringType == type;
            var prefix = isDeclared ? "" : "[verbose](inherited) [/]";
            logger.Write($"{indent}[tab]" + prefix);
            logger.Write(FormatTemplates.ShortMethodSyntax.Format(
                GetMemberModifiers(method),
                method.ReturnType.Name,
                method.Name,
                FormatParameters(method)
                ));
        }
        foreach (var nestedType in nestedTypes.OrderBy(t => t.Name))
        {
            if (indentLevel < 5)
            {
                DumpType(nestedType, indentLevel + 1);
            }
            else
            {
                var nestedName = nestedType switch 
                {
                    { IsInterface: true } => $"[aqua]{nestedType.Name}[/]",
                    { IsEnum: true } or { IsValueType: true } => $"[pink]{nestedType.Name}[/]",
                    _ => $"[purple]{nestedType.Name}[/]"
                };
                logger.Write($"[br]{indent}[tab][blue]{GetMemberModifiers(nestedType)}[/] ");
                logger.Write($"{nestedName}{UIStrings.NestedPlaceholder}");
            }
        }
        logger.Write($"{indent}}}[br]");
    }
    
    private static string FormatParameters(MethodBase method)
    {
        var parameters = method.GetParameters().Select(p => 
            FormatTemplates.ParameterSyntax.Format(p.ParameterType.Name, p.Name));
        return string.Join(", ", parameters);
    }
    
    private static bool IsDelegate(Type type) => 
        type is { IsClass: true, IsSealed: true } && type.BaseType == typeof(MulticastDelegate);
    
    private static string GetMemberModifiers(MemberInfo? member)
    {
        if (member is not (Type or MethodBase or FieldInfo)) return "";
        var type = member as Type;
        dynamic target = member;
        var modifiers = new[] {
            // Visibility
            member switch
            {
                Type => type switch 
                {
                    { IsPublic: true } or { IsNestedPublic: true } => Modifiers.Public,
                    { IsNestedPrivate: true } => Modifiers.Private,
                    { IsNestedFamily: true } => Modifiers.Protected,
                    { IsNestedAssembly: true } or { IsNotPublic: true } => Modifiers.Internal,
                    { IsNestedFamORAssem: true } => Modifiers.ProtectedInternal,
                    _ => "" 
                },
                _ => target switch 
                {
                    _ when target.IsPublic => Modifiers.Public,
                    _ when target.IsPrivate => Modifiers.Private,
                    _ when target.IsFamily => Modifiers.Protected,
                    _ when target.IsAssembly => Modifiers.Internal,
                    _ when target.IsFamilyOrAssembly => Modifiers.ProtectedInternal,
                    _ => ""
                }
            },
            // Class type
            member is Type ? type switch
            {
                { IsAbstract: true, IsSealed: true } => Modifiers.Static,
                { IsAbstract: true, IsInterface: false } => Modifiers.Abstract,
                { IsSealed: true, IsValueType: false, IsEnum: false } => Modifiers.Sealed,
                _ => ""
            } : "",
            member is Type ? type switch
            {
                { IsInterface: true } => Modifiers.Interface,
                { IsEnum: true } => Modifiers.Enum,
                { IsValueType: true, IsEnum: false } => Modifiers.Struct,
                _ when IsDelegate(type!) => Modifiers.Delegate,
                _ => Modifiers.Class
            } : "",
            // Static for non-types and non-constructors
            member is not (Type or ConstructorInfo) && target.IsStatic ? Modifiers.Static : "",
            // Method modifiers
            member is MethodBase method and not ConstructorInfo ? method switch
            {
                { IsAbstract: true } => Modifiers.Abstract,
                { IsVirtual: true, IsFinal: false } => Modifiers.Virtual,
                _ => ""
            } : "",
            // Field modifiers
            member is FieldInfo && target.IsInitOnly ? Modifiers.Readonly : "",
            member is FieldInfo && target.IsLiteral ? Modifiers.Const : ""
        };
        return string.Join(" ", modifiers.Where(s => !string.IsNullOrWhiteSpace(s)));
    }
    
    #endregion
}