namespace ComponentInspector.Core.Interfaces;

public interface ILogger
{
    #region Interface properties
    
    bool IsVerboseEnabled { get; set; }
    
    #endregion // =================================================================================
    
    #region Interface methods

    void Write(string text);
    
    #endregion
}