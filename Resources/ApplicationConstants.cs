namespace ComponentInspector.Resources;

public static class ApplicationConstants
{
    #region Constants
    
    public const string 
        NewLine               =  "\n",
        Tab                   =  "    ",
        DefaultComponentPath  =  "example.dll";
    
    public static class CommandLineOptions
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
            AllShort         =  "-a";
    }
    
    #endregion // =================================================================================
    
    #region Readonly fields
    
    public static readonly Dictionary<string, ConsoleColor> Colors = new()
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
    
    #endregion
}