using ComponentInspector.Models;

namespace ComponentInspector.Core.Interfaces;

public interface IArgumentParser
{
    #region Interface methods
    
    ApplicationConfiguration Parse(string[] args);
    
    #endregion
}