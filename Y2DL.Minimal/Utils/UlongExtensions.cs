namespace Y2DL.Minimal.Utils;

public static class UlongExtensions
{
    public static string ToFormattedNumber(this ulong? num)
    {
        var thousand = 1000.0;
        var million = 1000000.0;
        var billion = 1000000000.0;

        double number = num ?? 0;

        if (Math.Abs(number) >= billion)
            return (number / billion).ToString("0.00") + "B";
        if (Math.Abs(number) >= million)
            return (number / million).ToString("0.00") + "M";
        if (Math.Abs(number) >= thousand)
            return (number / thousand).ToString("0.00") + "K";
        return number.ToString();
    }

    public static ulong? ToUlong(this ulong? @ulong)
    {
        return (ulong?)@ulong;
    }

    public static ulong? ToUlong(this long? @long)
    {
        return (ulong?)@long;
    }
    
    public static ulong ToUlong(this long @long)
    {
        return (ulong)@long;
    }
}