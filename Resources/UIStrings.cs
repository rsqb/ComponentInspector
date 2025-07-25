namespace ComponentInspector.Resources;

// ReSharper disable once InconsistentNaming
internal static class UIStrings
{
    #region Constants

    internal const string
        TypesSection       =  "[br][dim]*[/] [purple]Types[/] [white]in assembly[/]:[br]",
        FieldsSection      =  "[br][dim]*[/] [sky]Fields[/] [white]in assembly[/]:[br]",
        PropertiesSection  =  "[br][dim]*[/] [sky]Properties[/] [white]in assembly[/]:[br]",
        MethodsSection     =  "[br][dim]*[/] [teal]Methods[/] [white]in assembly[/]:[br][br]",
        DumpSection        =  "[br][dim]*[/] [white]Assembly structure[/] [purple](C# Style)[/]:[br]",
        NonePresent        =  "[tab][dim]None present[/][br]",
        NoResult           =  "None[verbose] (possibly console output)[/]",
        NewHint            =  "[verbose]new: [/]",
        CachedHint         =  "[verbose]cached: [/]",
        EmptyTypeBody      =  " {}[br]",
        NestedPlaceholder  =  " {{ ... }}[br]",
        ExitPrompt         =  "[br]Press any key to exit... ";
    
    internal static class ErrorMessages
    {
        internal const string
            MethodNameRequired   =  "Method invocation (-i/--invoke) requires a method name",
            InvalidMethodFormat  =  "Invalid method name format. Use: FullTypeName.MethodName",
            NoStackTrace         =  "No stack trace available",
            NoAssemblyLoaded     =  "No assembly has been loaded into inspector. " +
                                    "Call InspectAssembly first.";
    }
    
    #endregion
}