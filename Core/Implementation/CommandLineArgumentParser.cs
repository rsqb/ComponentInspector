using ComponentInspector.Core.Interfaces;
using ComponentInspector.Models;
using ComponentInspector.Resources;

namespace ComponentInspector.Core.Implementation;

using Option = ApplicationConstants.CommandLineOptions;

internal class CommandLineArgumentParser(ILogger logger) : IArgumentParser
{
    #region Methods
    
    public ApplicationConfiguration Parse(string[] args)
    {
        var builder = new ApplicationConfiguration.Builder();
        var positionalArgs = new List<string>();
        string[] priority = [Option.VerboseShort, Option.Verbose, Option.HelpShort, Option.Help];
        args = priority.Where(args.Contains).Concat(args.Where(x => !priority.Contains(x))).ToArray();
        Console.WriteLine(string.Join(", ", ApplicationConstants.AllOptions));
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg.IsKnownOption())
            {
                switch (arg.ToLower())
                {
                    case Option.HelpShort:
                    case Option.Help:
                        builder.WithHelp();
                        goto BuildConfig;
                    case Option.VerboseShort:
                    case Option.Verbose:
                        builder.WithVerbose();
                        break;
                    case Option.TypesShort:
                    case Option.Types:
                        builder.WithTypes();
                        break;
                    case Option.FieldsShort:
                    case Option.Fields:
                        builder.WithFields();
                        break;
                    case Option.PropertiesShort:
                    case Option.Properties:
                        builder.WithProperties();
                        break;
                    case Option.MethodsShort:
                    case Option.Methods:
                        builder.WithMethods();
                        break;
                    case Option.AllShort:
                    case Option.All:
                        builder.WithFullDump();
                        break;
                    case Option.InvokeShort:
                    case Option.Invoke:
                        var invocation = ParseMethodInvocation(args, ref i);
                        if (invocation != null)
                            builder.WithMethodInvocation(invocation);
                        break;
                }
            }
            else
            {
                positionalArgs.Add(arg);
            }
        }
        if (positionalArgs.Count > 0)
        {
            builder.WithComponent(positionalArgs[0]);
        }
        BuildConfig:
        var config = builder.Build();
        logger.IsVerboseEnabled = config.IsVerbose;
        logger.Write(FormatTemplates.ProgramArguments.Format(
            string.Join(" ", args),
            config.ComponentFile?.FullName ?? "(none)",
            config.IsVerbose,
            config.ShowHelp,
            config.ListTypes,
            config.ListFields,
            config.ListProperties,
            config.ListMethods
            ));
        return config;
    }
    
    private MethodInvocation? ParseMethodInvocation(string[] args, ref int currentIndex)
    {
        if (currentIndex + 1 >= args.Length || args[currentIndex + 1].IsKnownOption())
        {
            throw new ArgumentException("Method invocation (-i/--invoke) requires a method name");
        }
        var methodName = args[++currentIndex];
        var arguments = new List<string>();
        while (currentIndex + 1 < args.Length && !args[currentIndex + 1].IsKnownOption())
        {
            arguments.Add(args[++currentIndex]);
        }
        return new MethodInvocation(methodName, arguments.ToArray());
    }
    
    #endregion
}