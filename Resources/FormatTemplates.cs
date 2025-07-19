namespace ComponentInspector.Resources;

internal static class FormatTemplates
{
    #region Constants
    
    internal const string
        LoadingComponent    =  "[info]Loading component: {0}[/]",
        AssemblyLoaded      =  "[success]Successfully loaded assembly: {0}[/]",
        FoundTypesAmount    =  "[yellow]Found {0} type(s) in the assembly[/][br]",
        CompletionTime      =  "[br][info]Component inspection completed in {0}[/]",
        NamespaceSyntax     =  "[blue]namespace[/] [purple]{0}[/][br]",
        InterfaceSyntax     =  "[purple]{0}[/][br]",
        TypeSyntax          =  "{0}[purple]{1}[/]{2}",
        FieldSyntax         =  "[blue]{0}[/] [pink]{1}[/] [sky]{2}[/][br]",
        PropertySyntax      =  "[blue]{0}[/] [pink]{1}[/] [sky]{2}[/] {{[teal]{3}[/]{4}[teal]{5}[/] }}[br]",
        ConstructorSyntax   =  "[blue]{0}[/] [purple]{1}[/]({2})[br]",
        MethodSyntax        =  "[blue]{0}[/] [pink]{1}[/] [purple]{2}[/].[teal]{3}[/]({4})[br]",
        ShortMethodSyntax   =  "[blue]{0}[/] [pink]{1}[/] [teal]{2}[/]({3})[br]",
        ParameterSyntax     =  "[pink]{0}[/] {1}",
        ParamHintSyntax     =  "[verbose]{0} {1}: [/]{2}",
        StringSyntax        =  "\"{0}\"",
        CharSyntax          =  "'{0}'",
        TypeHeader          =  "[br][tab][purple]{0}[/]:[br]",
        InterfacesHeader    =  "[tab][tab]Implemented Interfaces:[br]",
        FieldsHeader        =  "[tab][tab]Fields:[br]",
        PropertiesHeader    =  "[tab][tab]Properties:[br]",
        ConstructorsHeader  =  "[tab][tab]Constructors:[br]",
        MethodsHeader       =  "[tab][tab]Methods:[br]",
        InvocationDetails   =  "[br][dim]>>[/] [teal]Invoking:[/] {0}.[sky]{1}[/]({2})[br]",
        ResultDetails       =  "[dim]>>[/] [sky]Result:[/] {0}[br]",
        TypeDetails         =  """
                               [tab][tab]Is Public: [blue]{0}[/]
                               [tab][tab]Is Internal: [blue]{1}[/]
                               [tab][tab]Is Class: [blue]{2}[/]
                               [tab][tab]Is Interface: [blue]{3}[/]
                               [tab][tab]Is Abstract: [blue]{4}[/]
                               [tab][tab]Is Sealed: [blue]{5}[/]
                               [tab][tab]Is Static: [blue]{6}[/]
                               [tab][tab]Is ValueType: [blue]{7}[/]
                               [tab][tab]Is Enum: [blue]{8}[/]
                               [tab][tab]Base Type: [pink]{9}[/]
                               
                               """,
        ProgramArguments    =  """
                               
                               [verbose]Command Line Arguments:
                               [tab]* Original Args: "{0}"
                               [tab]* Component: "{1}"
                               [tab]* Verbose: {2}
                               [tab]* Show Help: {3}
                               [tab]* Show Types: {4}
                               [tab]* Show Fields: {5}
                               [tab]* Show Properties: {6}
                               [tab]* Show Methods: {7}
                               
                               [/]
                               """;
    
    internal static class Logging
    {
        internal const string
            Success  =  "[olive]+[/] [green]{0}[/][br]",
            Error    =  "[br][maroon]x[/] [red]Error : {0}[/][br]",
            Warning  =  "[yellow]![/] [gold]Warning : {0}[/][br]",
            Info     =  "[blue]i[/] [sky]{0}[/][br]",
            Verbose  =  "[dim]{0}[/]";
    }
    
    internal static class ErrorMessages
    {
        internal const string
            FileNotFound           =  "File not found: {0}",
            InvalidFileType        =  "Invalid file type: '{0}'. Expected .dll or .exe file.",
            InvalidDotNetAssembly  =  "'{0}' is not a valid .NET assembly or has an incorrect format.",
            FailedToLoadAssembly   =  "Failed to load assembly '{0}': {1}",
            FailedLoadFileMethod   =  "Failed to load with 'LoadFile()', trying 'LoadFrom()': {0}",
            TypeNotFound           =  "Type '{0}' not found in assembly or system types",
            MethodNotFound         =  "Method '{0}' not found in type '{1}'",
            NoOverloadFound        =  "No overload of '{0}' matches {1} arguments",
            CannotCreateAbstract   =  "Cannot create instance of abstract type '{0}'",
            ConversionFail         =  "Cannot convert '{0}' to {1}: {2}";
    }
    
    #endregion
}