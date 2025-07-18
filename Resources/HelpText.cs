namespace ComponentInspector.Resources;

using Opt = ApplicationConstants.CommandLineOptions;

internal static class HelpText
{
    #region Readonly fields

    internal static readonly string Content = $"""
[blue]Component Inspector[/] - [purple].NET Assembly Analysis Tool[/]

[white]Usage[/]: ComponentInspector.exe [dim][options][/] [dim][<dll-path>][/]

[white]Arguments[/]:
[tab]{"[sky]<dll-path>[/]",-27} Path to the DLL component to inspect
[tab]{"",-19} [blue](default: {ApplicationConstants.DefaultComponentPath})[/]

[white]Options[/]:
[tab]{$"[olive]{Opt.HelpShort}[/], [green]{Opt.Help}[/]",-39} Show this help information
[tab]{$"[gray]{Opt.VerboseShort}[/], [white]{Opt.Verbose}[/]",-38} Enable verbose logging
[tab]{$"[pink]{Opt.TypesShort}[/], [purple]{Opt.Types}[/]",-39} List all types
[tab]{$"[blue]{Opt.FieldsShort}[/], [sky]{Opt.Fields}[/]",-36} List all fields
[tab]{$"[blue]{Opt.PropertiesShort}[/], [sky]{Opt.Properties}[/]",-36} List all properties
[tab]{$"[aqua]{Opt.MethodsShort}[/], [teal]{Opt.Methods}[/]",-37} List all methods
[tab]{$"[gold]{Opt.AllShort}[/], [yellow]{Opt.All}[/]",-39} Show complete assembly structure in C# style
[tab]{$"[aqua]{Opt.InvokeShort}[/], [teal]{Opt.Invoke}[/]",-37} Invoke method(s) with optional arguments
[tab]{"",-19} [blue](syntax: [/][purple]full_type_name[/].[teal]method_name[/] [dim]arg1 arg2 ...[/][blue])[/]

""";

    #endregion
}