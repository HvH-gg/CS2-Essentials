namespace hvhgg_essentials.Extensions;

public static class QAngleExtensions
{
    public static bool IsInfinity(this CounterStrikeSharp.API.Modules.Utils.QAngle angle)
    {
        return float.IsInfinity(angle.X) || float.IsInfinity(angle.Y) || float.IsInfinity(angle.Z);
    }
    public static void FixInfinity(this CounterStrikeSharp.API.Modules.Utils.QAngle angle)
    {
        if (float.IsInfinity(angle.X))
            angle.X = 0;
        if (float.IsInfinity(angle.Y))
            angle.Y = 0;
        if (float.IsInfinity(angle.Z))
            angle.Z = 0;
    }
}