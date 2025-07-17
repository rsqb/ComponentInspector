using System.Reflection;
using ComponentInspector.Resources;

namespace ComponentInspector.Models;

using ErrorTemplate = FormatTemplates.ErrorMessages;

public record ApplicationConfiguration
{
    #region Properties
    
    public bool ShowHelp { get; }
    public bool IsVerbose { get; }
    public FileInfo? ComponentFile { get; }
    public bool ListTypes { get; }
    public bool ListFields { get; }
    public bool ListProperties { get; }
    public bool ListMethods { get; }
    
    #endregion // =================================================================================

    #region Constructor
    
    private ApplicationConfiguration(
        bool showHelp, bool isVerbose, FileInfo? componentFile,
        bool listTypes, bool listFields, bool listProperties, bool listMethods)
    {
        ShowHelp = showHelp;
        IsVerbose = isVerbose;
        ComponentFile = componentFile;
        ListTypes = listTypes;
        ListFields = listFields;
        ListProperties = listProperties;
        ListMethods = listMethods;
    }
    
    #endregion // =================================================================================
    
    public sealed class Builder
    {
        #region Fields
        
        private bool _showHelp;
        private bool _isVerbose;
        private string? _componentPath;
        private bool _listTypes;
        private bool _listFields;
        private bool _listProperties;
        private bool _listMethods;
        
        #endregion // =============================================================================
        
        #region Builder pattern
        
        public Builder WithHelp(bool value = true)
        {
            _showHelp = value;
            return this;
        }
        
        public Builder WithVerbose(bool value = true)
        {
            _isVerbose = value;
            return this;
        }
        
        public Builder WithComponent(string path)
        {
            _componentPath = path;
            return this;
        }
        
        public Builder WithTypes(bool value = true)
        {
            _listTypes = value;
            return this;
        }
        
        public Builder WithFields(bool value = true)
        {
            _listFields = value;
            return this;
        }
        
        public Builder WithProperties(bool value = true)
        {
            _listProperties = value;
            return this;
        }
        
        public Builder WithMethods(bool value = true)
        {
            _listMethods = value;
            return this;
        }
        
        public Builder WithFullDump()
        {
            _listTypes = true;
            _listFields = true;
            _listProperties = true;
            _listMethods = true;
            return this;
        }
        
        public ApplicationConfiguration Build()
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
                _showHelp, _isVerbose, componentFile, 
                _listTypes, _listFields, _listProperties, _listMethods
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
                throw new BadImageFormatException(ErrorTemplate.InvalidDotNetAssembly.Format(file.Name));
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