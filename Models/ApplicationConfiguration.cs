using System.Reflection;
using ComponentInspector.Resources;

namespace ComponentInspector.Models;

using ErrorTemplate = FormatTemplates.ErrorMessages;

internal record ApplicationConfiguration
{
    #region Properties
    
    internal bool ShowHelp { get; }
    internal bool IsVerbose { get; }
    internal bool ListTypes { get; }
    internal bool ListFields { get; }
    internal bool ListProperties { get; }
    internal bool ListMethods { get; }
    internal FileInfo? ComponentFile { get; }
    internal List<MethodInvocation> MethodInvocations { get; }
    
    #endregion // =================================================================================

    #region Constructor
    
    private ApplicationConfiguration(
        bool showHelp, bool isVerbose, 
        bool listTypes, bool listFields, bool listProperties, bool listMethods,
        FileInfo? componentFile, List<MethodInvocation> methodInvocations)
    {
        ShowHelp = showHelp;
        IsVerbose = isVerbose;
        ListTypes = listTypes;
        ListFields = listFields;
        ListProperties = listProperties;
        ListMethods = listMethods;
        ComponentFile = componentFile;
        MethodInvocations = methodInvocations;
    }
    
    #endregion // =================================================================================
    
    internal sealed class Builder
    {
        #region Fields
        
        private bool _showHelp;
        private bool _isVerbose;
        private bool _listTypes;
        private bool _listFields;
        private bool _listProperties;
        private bool _listMethods;
        private string? _componentPath;
        private readonly List<MethodInvocation> _methodInvocations = [];
        
        #endregion // =============================================================================
        
        #region Builder pattern
        
        internal Builder WithHelp(bool value = true)
        {
            _showHelp = value;
            return this;
        }
        
        internal Builder WithVerbose(bool value = true)
        {
            _isVerbose = value;
            return this;
        }
        
        internal Builder WithComponent(string path)
        {
            _componentPath = path;
            return this;
        }
        
        internal Builder WithTypes(bool value = true)
        {
            _listTypes = value;
            return this;
        }
        
        internal Builder WithFields(bool value = true)
        {
            _listFields = value;
            return this;
        }
        
        internal Builder WithProperties(bool value = true)
        {
            _listProperties = value;
            return this;
        }
        
        internal Builder WithMethods(bool value = true)
        {
            _listMethods = value;
            return this;
        }
        
        internal Builder WithFullDump()
        {
            _listTypes = true;
            _listFields = true;
            _listProperties = true;
            _listMethods = true;
            return this;
        }
        
        internal Builder WithMethodInvocation(MethodInvocation invocation)
        {
            _methodInvocations.Add(invocation);
            return this;
        }
        
        internal ApplicationConfiguration Build()
        {
            FileInfo? componentFile = null;
            if (!_showHelp)
            {
                componentFile = ValidateAndGetComponentFile(string.IsNullOrEmpty(_componentPath)
                    ? ApplicationConstants.DefaultComponentPath
                    : _componentPath
                    );
            }
            return new ApplicationConfiguration(
                _showHelp, _isVerbose, _listTypes, _listFields, _listProperties, _listMethods, 
                componentFile, _methodInvocations
                );
        }

        #endregion // =============================================================================
        
        #region Helper methods

        private static FileInfo ValidateAndGetComponentFile(string path)
        {
            var file = new FileInfo(path);
            if (!file.Exists)
            {
                throw new FileNotFoundException(ErrorTemplate.FileNotFound.Format(file.FullName));
            }
            if (!file.Extension.Equals(".dll", StringComparison.OrdinalIgnoreCase) &&
                !file.Extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    ErrorTemplate.InvalidFileType.Format(file.Extension), nameof(path));
            }
            try
            {
                AssemblyName.GetAssemblyName(file.FullName);
            }
            catch (BadImageFormatException)
            { 
                throw new BadImageFormatException(
                    ErrorTemplate.InvalidDotNetAssembly.Format(file.Name));
            }
            catch (FileLoadException ex)
            {
                throw new FileLoadException(
                    ErrorTemplate.FailedToLoadAssembly.Format(file.Name, ex.Message), file.FullName, ex);
            }
            return file;
        }
        
        #endregion
    }
}