using SmartFormat.Core.Extensions;
using Y2DL.Minimal.Utils;

namespace Y2DL.Minimal.SmartFormatters;

public class ToSnowflakeFormatter: IFormatter
{
    public string Name { get; set; } = "ToSnowflake";
    public bool CanAutoDetect { get; set; } = false;

    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        if (!(formattingInfo.CurrentValue is DateTimeOffset? || formattingInfo.CurrentValue is DateTimeOffset))
            return false;
        
        formattingInfo.Write(((DateTimeOffset)formattingInfo.CurrentValue).ToUnixTimeSeconds().ToString());

        return true;
    }
}