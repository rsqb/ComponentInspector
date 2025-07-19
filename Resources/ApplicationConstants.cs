using System.Reflection;

namespace ComponentInspector.Resources;

internal static class ApplicationConstants
{
    #region Constants
    
    internal const string 
        NewLine               =  "\n",
        Tab                   =  "    ",
        DefaultArgPrefix      =  "arg",
        NullString            =  "null",
        DefaultNamespace      =  "Global",
        DefaultComponentPath  =  "example.dll"; 
    
    internal static class SystemTypes
    {
        internal const string
            String         =  "String",
            Char           =  "Char",
            Boolean        =  "Boolean",
            Math           =  "Math",
            Console        =  "Console",
            SystemPrefix   =  "System.",
            SystemConsole  =  "System.Console",
            SystemRuntime  =  "System.Runtime",
            MsCorLib       =  "mscorlib";
    }
    
    internal static class CommandLineOptions
    {
        internal const string
            Help             =  "--help", 
            HelpShort        =  "-h", 
            Verbose          =  "--verbose",
            VerboseShort     =  "-v", 
            Types            =  "--types", 
            TypesShort       =  "-t",
            Fields           =  "--fields", 
            FieldsShort      =  "-f",
            Properties       =  "--properties",
            PropertiesShort  = "-p",
            Methods          =  "--methods",
            MethodsShort     =  "-m",
            All              =  "--all",
            AllShort         =  "-a",
            Invoke           =  "--invoke",
            InvokeShort      =  "-i";
    }
    
    internal static class Modifiers
    {
        internal const string
            Public             =  "public",
            Private            =  "private",
            Protected          =  "protected",
            Internal           =  "internal",
            ProtectedInternal  =  "protected internal",
            Static             =  "static",
            Abstract           =  "abstract",
            Sealed             =  "sealed",
            Interface          =  "interface",
            Enum               =  "enum",
            Struct             =  "struct",
            Delegate           =  "delegate",
            Class              =  "class",
            Virtual            =  "virtual",
            Readonly           =  "readonly",
            Const              =  "const";
    }
    
    #endregion // =================================================================================
    
    #region Readonly fields
    
    internal static readonly Dictionary<string, ConsoleColor> Colors = new()
    {
        ["white"]   =  ConsoleColor.White,
        ["dim"]     =  ConsoleColor.DarkGray,
        ["gray"]    =  ConsoleColor.Gray,
        ["black"]   =  ConsoleColor.Black,
        ["red"]     =  ConsoleColor.Red,
        ["maroon"]  =  ConsoleColor.DarkRed,
        ["green"]   =  ConsoleColor.Green,
        ["olive"]   =  ConsoleColor.DarkGreen,
        ["sky"]     =  ConsoleColor.Blue,
        ["blue"]    =  ConsoleColor.DarkBlue,
        ["aqua"]    =  ConsoleColor.Cyan,
        ["teal"]    =  ConsoleColor.DarkCyan,
        ["gold"]    =  ConsoleColor.Yellow,
        ["yellow"]  =  ConsoleColor.DarkYellow,
        ["pink"]    =  ConsoleColor.Magenta,
        ["purple"]  =  ConsoleColor.DarkMagenta
    };
    
    internal static readonly HashSet<string> AllOptions = typeof(CommandLineOptions)
        .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        .Select(f => (string)f.GetValue(null)!)
        .ToHashSet();
    
    #endregion
}