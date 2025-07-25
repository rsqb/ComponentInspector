﻿/*
 *  Simple .NET Reflection-based tool for inspecting small .NET assemblies.
 *
 *  Usage:
 *      ComponentInspector.exe [<dll-path>] [options]
 * 
 *  Arguments:
 *      <dll-path>          Path to the DLL or EXE component to inspect, default is: 'example.dll'
 * 
 *  Options:
 *      -h, --help          Show help information
 *      -v, --verbose       Enable verbose logging
 *      -t, --types         List all types in the component
 *      -f, --fields        List all fields in the component
 *      -p, --properties    List all properties in the component
 *      -m, --methods       List all methods in the component
 *      -a, --all           Show complete assembly structure in C# style
 *      -i, --invoke        Invoke method(s) with optional arguments, syntax is: 
 *                          full_type_name.method_name arg1 arg2 ...
 */

using ComponentInspector.Core.Application;
using ComponentInspector.Core.Implementation;
using ComponentInspector.Resources;

#region Main procedure

// Set up dependency injection manually
var logger = new ConsoleLogger();
var argumentParser = new CommandLineArgumentParser(logger);
var inspector = new ReflectionComponentInspector(logger);
var application = new ComponentInspectorApplication(logger, argumentParser, inspector);

// Run the application
var exitCode = application.Run(args);

// Wait for user input before exiting
if (Environment.UserInteractive)
{
    logger.Write(UIStrings.ExitPrompt);
    Console.ReadKey();
    Console.WriteLine();
}

return exitCode;

#endregion