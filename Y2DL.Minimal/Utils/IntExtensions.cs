namespace Y2DL.Minimal.Utils;

public static class IntExtensions
{
    public static double Round(this double input, int dec = 0)
    {
        return Math.Round(input, dec);
    }

    public static ulong? ToUlong(this int input) {
        return (ulong)input;
    }
}