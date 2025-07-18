namespace ComponentInspector.Resources;

internal static class StringExtensions
{
    #region Helper methods
    
    internal static bool IsKnownOption(this string val) => ApplicationConstants.AllOptions.Contains(val);
    
    internal static string Format(this string template, params object?[] args) => 
        string.Format(template, args);
    
    #endregion
}