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
        var priority = new[] { Option.VerboseShort, Option.Verbose, Option.HelpShort, Option.Help };
        foreach (var arg in priority.Where(args.Contains).Concat(args.Except(priority)))
        {
            if (arg.StartsWith('-'))
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
    
    #endregion
}