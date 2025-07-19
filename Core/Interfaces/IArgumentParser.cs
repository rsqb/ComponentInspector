using ComponentInspector.Models;

namespace ComponentInspector.Core.Interfaces;

internal interface IArgumentParser
{
    #region Interface methods
    
    ApplicationConfiguration Parse(string[] args);
    
    #endregion
}