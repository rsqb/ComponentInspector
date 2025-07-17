using System.Text.RegularExpressions;
using ComponentInspector.Core.Interfaces;
using ComponentInspector.Resources;

namespace ComponentInspector.Core.Implementation;

internal partial class ConsoleLogger : ILogger
{
    #region Fields
    
    private static readonly Regex TagPattern = TagRegex();
    
    #endregion // =================================================================================
    
    #region Properties
    
    public bool IsVerboseEnabled { get; set; } = true;
    
    #endregion // =================================================================================

    #region Methods
    
    [GeneratedRegex(@"\[([\w-]+)\]([\s\S]*?)\[\/\]", RegexOptions.Compiled)]
    private static partial Regex TagRegex();
    
    public void Write(string text) => ProcessColorTags(ExpandSpecialTags(text));
    
    private string ExpandSpecialTags(string text)
    {
        text = text
            .Replace("[tab]", ApplicationConstants.Tab)
            .Replace("[br]", ApplicationConstants.NewLine);
        var matches = TagPattern.Matches(text).ToList();
        for (var i = matches.Count - 1; i >= 0; i--)
        {
            var match = matches[i];
            var tag = match.Groups[1].Value.ToLower();
            var content = match.Groups[2].Value;
            var replacement = tag switch
            {
                "success" => FormatTemplates.Logging.Success.Format(content),
                "error" => FormatTemplates.Logging.Error.Format(content),
                "warning" => FormatTemplates.Logging.Warning.Format(content),
                "info" => FormatTemplates.Logging.Info.Format(content),
                "verbose" => IsVerboseEnabled
                    ? FormatTemplates.Logging.Verbose.Format(content) 
                    : string.Empty,
                _ => null
            };
            if (replacement != null)
            {
                text = text.Remove(match.Index, match.Length).Insert(match.Index, replacement);
            }
        }
        return text
            .Replace("[tab]", ApplicationConstants.Tab)
            .Replace("[br]", ApplicationConstants.NewLine);
    }
    
    private static void ProcessColorTags(string text)
    {
        var lastIndex = 0;
        foreach (Match match in TagPattern.Matches(text))
        {
            if (match.Index > lastIndex)
            {
                Console.Write(text.Substring(lastIndex, match.Index - lastIndex));
            }
            lastIndex = match.Index + match.Length;
            var tag = match.Groups[1].Value.ToLower();
            var content = match.Groups[2].Value;
            if (ApplicationConstants.Colors.TryGetValue(tag, out var color))
            {
                var oldColor = Console.ForegroundColor; 
                Console.ForegroundColor = color; 
                Console.Write(content); 
                Console.ForegroundColor = oldColor;
            }
            else
            { 
                Console.Write(content);
            }
        }
        if (lastIndex < text.Length)
        {
            Console.Write(text[lastIndex..]);
        }
    }
    
    #endregion
}