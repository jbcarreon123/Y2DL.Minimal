using SmartFormat.Core.Extensions;
using Y2DL.Minimal.Utils;

namespace Y2DL.Minimal.SmartFormatters;

public class LimitFormatter : IFormatter
{
    public string Name { get; set; } = "Limit";
    public bool CanAutoDetect { get; set; } = false;

    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        if (!(formattingInfo.CurrentValue is string))
            return false;

        if (!int.TryParse(formattingInfo.FormatterOptions, out var opt))
            return false;
        
        formattingInfo.Write((formattingInfo.CurrentValue as string).Limit(opt));

        return true;
    }
}