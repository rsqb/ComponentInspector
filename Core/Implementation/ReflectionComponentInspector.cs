using System.Reflection;
using ComponentInspector.Core.Interfaces;
using ComponentInspector.Resources;

namespace ComponentInspector.Core.Implementation;

internal class ReflectionComponentInspector(ILogger logger) : IComponentInspector
{
    #region Fields
    
    private const BindingFlags MemberDiscoveryFlags = BindingFlags.Public | BindingFlags.NonPublic | 
                                                      BindingFlags.Instance | BindingFlags.Static | 
                                                      BindingFlags.DeclaredOnly;
    
    private Assembly? _assembly;
    private Type[]? _types;
    
    #endregion // =================================================================================
    
    #region Implementation methods

    public void InspectAssembly(Assembly assembly)
    {
        _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
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
                    logger.Write("[tab][tab][tab]" + FormatTemplates.InterfaceSyntax.Format(i.FullName));
                }
            }
            // Fields
            var fields = type.GetFields(MemberDiscoveryFlags);
            if (fields.Length > 0)
            {
                logger.Write(FormatTemplates.FieldsHeader);
                foreach (var field in fields)
                {
                    logger.Write("[tab][tab][tab]" + FormatTemplates.FieldSyntax.Format(
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
                    logger.Write("[tab][tab][tab]" + FormatTemplates.PropertySyntax.Format(
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
                    logger.Write("[tab][tab][tab]" + FormatTemplates.ConstructorSyntax.Format(
                        GetMemberModifiers(ctor),
                        type.Name,
                        FormatParameters(ctor)
                    ));
                }
            }
            // Methods
            var methods = type.GetMethods(MemberDiscoveryFlags).Where(m => !m.IsSpecialName).ToArray();
            if (methods.Length > 0)
            {
                logger.Write(FormatTemplates.MethodsHeader); 
                foreach (var method in methods)
                {
                    logger.Write("[tab][tab][tab]" + FormatTemplates.ShortMethodSyntax.Format(
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
        (_, field) => FormatTemplates.FieldSyntax.Format(
            GetMemberModifiers(field), 
            field.FieldType.Name, 
            field.Name
            )
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
        type => type.GetMethods(MemberDiscoveryFlags).Where(method => !method.IsSpecialName).ToArray(),
        (type, member) => FormatTemplates.MethodSyntax.Format(
            GetMemberModifiers(member),
            member.ReturnType.Name,
            type.Name, 
            member.Name, 
            FormatParameters(member)
            ),
        groupByType: false
        );
    
    public void DumpAll()
    {
        EnsureAssemblyLoaded();
        logger.Write(UIStrings.DumpSection);
        var namespaceGroups = _types!
            .Where(t => !t.IsNested)
            .GroupBy(t => t.Namespace ?? "Global")
            .OrderBy(g => g.Key);
        foreach (var namespaceGroup in namespaceGroups)
        {
            logger.Write($"[br][tab]{FormatTemplates.NamespaceSyntax.Format(namespaceGroup.Key)}[tab]{{");
            foreach (var type in namespaceGroup)
            {
                DumpType(type, 2);
            }
            logger.Write("[tab]}[br]");
        }
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
                logger.Write((groupByType ? "[tab][tab]" : "[tab]") + memberFormatter(type, member));
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
            { IsInterface: true } => $"[teal]{type.Name}[/]",
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
                    .Select(i => $"[teal]{i.Name}[/]")
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
        var methods = type.GetMethods(MemberDiscoveryFlags).Where(m => !m.IsSpecialName).ToArray();
        var nestedTypes = type.GetNestedTypes(MemberDiscoveryFlags);
        var hasContent = enumValues?.Length > 0 || fields.Length > 0 || properties.Length > 0 ||
                         constructors.Length > 0 || methods.Length > 0 || nestedTypes.Length > 0;
        if (!hasContent)
        {
            logger.Write(" {}[br]");
            return;
        }
        logger.Write($"[br]{indent}{{");
        if (type.IsEnum && enumValues!.Length > 0)
        {
            for (var i = 0; i < enumValues.Length; i++)
            {
                logger.Write($"[br]{indent}[tab][pink]{enumValues[i]}[/]");
                if (i < enumValues.Length - 1) logger.Write(",");
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
            logger.Write($"{indent}[tab]" + FormatTemplates.FieldSyntax.Format(
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
            logger.Write($"{indent}[tab]" + FormatTemplates.PropertySyntax.Format(
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
            logger.Write($"{indent}[tab]" + FormatTemplates.ShortMethodSyntax.Format(
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
                    { IsInterface: true } => $"[teal]{nestedType.Name}[/]",
                    { IsEnum: true } or { IsValueType: true } => $"[pink]{nestedType.Name}[/]",
                    _ => $"[purple]{nestedType.Name}[/]"
                };
                logger.Write($"[br]{indent}[tab][blue]{GetMemberModifiers(nestedType)}[/] ");
                logger.Write($"{nestedName} {{ ... }}[br]");
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
                    { IsPublic: true } or { IsNestedPublic: true } => "public",
                    { IsNestedPrivate: true } => "private",
                    { IsNestedFamily: true } => "protected",
                    { IsNestedAssembly: true } or { IsNotPublic: true } => "internal",
                    { IsNestedFamORAssem: true } => "protected internal",
                    _ => "" 
                },
                _ => target switch 
                {
                    _ when target.IsPublic => "public",
                    _ when target.IsPrivate => "private",
                    _ when target.IsFamily => "protected",
                    _ when target.IsAssembly => "internal",
                    _ when target.IsFamilyOrAssembly => "protected internal",
                    _ => ""
                }
            },
            // Class type
            member is Type ? type switch
            {
                { IsAbstract: true, IsSealed: true } => "static",
                { IsAbstract: true, IsInterface: false } => "abstract",
                { IsSealed: true, IsValueType: false, IsEnum: false } => "sealed",
                _ => ""
            } : "",
            member is Type ? type switch
            {
                { IsInterface: true } => "interface",
                { IsEnum: true } => "enum",
                { IsValueType: true, IsEnum: false } => "struct",
                _ when IsDelegate(type!) => "delegate",
                _ => "class"
            } : "",
            // Static for non-types and non-constructors
            member is not (Type or ConstructorInfo) && target.IsStatic ? "static" : "",
            // Method modifiers
            member is MethodBase method and not ConstructorInfo ? method switch
            {
                { IsAbstract: true } => "abstract",
                { IsVirtual: true, IsFinal: false } => "virtual",
                _ => ""
            } : "",
            // Field modifiers
            member is FieldInfo && target.IsInitOnly ? "readonly" : "",
            member is FieldInfo && target.IsLiteral ? "const" : ""
        };
        return string.Join(" ", modifiers.Where(s => !string.IsNullOrWhiteSpace(s)));
    }
    
    #endregion
}