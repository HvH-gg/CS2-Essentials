namespace hvhgg_essentials.Extensions;

public static class QAngleExtensions
{
    public static bool IsInfinity(this CounterStrikeSharp.API.Modules.Utils.QAngle angle)
    {
        return float.IsInfinity(angle.X) || float.IsInfinity(angle.Y) || float.IsInfinity(angle.Z);
    }
    public static bool IsNaN(this CounterStrikeSharp.API.Modules.Utils.QAngle angle)
    {
        return float.IsNaN(angle.X) || float.IsNaN(angle.Y) || float.IsNaN(angle.Z);
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
    public static void FixNaN(this CounterStrikeSharp.API.Modules.Utils.QAngle angle)
    {
        if (float.IsNaN(angle.X))
            angle.X = 0;
        if (float.IsNaN(angle.Y))
            angle.Y = 0;
        if (float.IsNaN(angle.Z))
            angle.Z = 0;
    }
}