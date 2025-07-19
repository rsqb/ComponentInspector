using System.Diagnostics;
using System.Reflection;
using ComponentInspector.Core.Interfaces;
using ComponentInspector.Resources;

namespace ComponentInspector.Core.Application;

internal class ComponentInspectorApplication(
    ILogger logger, IArgumentParser argumentParser, IComponentInspector inspector)
{
    #region Methods
    
    internal int Run(string[] args)
    {          
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var config = argumentParser.Parse(args);
            if (config.ShowHelp)
            {
                logger.Write(HelpText.Content);
                return 0;
            }
            
            // Load the assembly
            logger.Write(FormatTemplates.LoadingComponent.Format(config.ComponentFile!.Name));
            var assembly = LoadAssembly(config.ComponentFile);
            logger.Write(FormatTemplates.AssemblyLoaded.Format(assembly.FullName));
            
            // Perform inspection
            inspector.InspectAssembly(assembly);
            
            // Execute requested operations
            if (config is { ListTypes: true, ListFields: true, ListProperties: true, ListMethods: true })
            {
                inspector.DumpAll();
            }
            else
            {
                if (config.ListTypes) inspector.ListTypes();
                if (config.ListFields) inspector.ListFields();
                if (config.ListProperties) inspector.ListProperties();
                if (config.ListMethods) inspector.ListMethods();
            }
            if (config.MethodInvocations.Count > 0)
            {
                foreach (var invocation in config.MethodInvocations)
                {
                    inspector.InvokeMethod(invocation);
                }
            }
            
            stopwatch.Stop();
            logger.Write(FormatTemplates.CompletionTime.Format(stopwatch.Elapsed));
            return 0;
        }
        catch (Exception ex)
        {
            logger.Write($"[error]{ex.Message}[/]");
            logger.Write($"[br][verbose]{ex.StackTrace ?? UIStrings.ErrorMessages.NoStackTrace}[/]");
            return 1;
        }
    }
    
    private Assembly LoadAssembly(FileInfo assemblyFile)
    {
        try
        {
            return Assembly.LoadFile(assemblyFile.FullName);
        }
        catch (Exception ex)
        {
            logger.Write(
                $"[warning]{FormatTemplates.ErrorMessages.FailedLoadFileMethod.Format(ex.Message)}[/]");
            return Assembly.LoadFrom(assemblyFile.FullName);
        }
    }
    
    #endregion
}