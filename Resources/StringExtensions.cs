namespace ComponentInspector.Resources;

internal static class StringExtensions
{
    #region Helper methods
    
    internal static string Format(this string template, params object?[] args) => 
        string.Format(template, args);
    
    #endregion
}