namespace hvhgg_essentials.Extensions;

public static class QAngleExtensions
{
    public static bool IsValid(this CounterStrikeSharp.API.Modules.Utils.QAngle angle)
    {
        return angle.IsFinite() && !angle.IsNaN() && angle.IsReasonable();
    }

    private static bool IsFinite(this CounterStrikeSharp.API.Modules.Utils.QAngle angle)
    {
        return float.IsFinite(angle.X) && float.IsFinite(angle.Y) && float.IsFinite(angle.Z);
    }

    private static bool IsNaN(this CounterStrikeSharp.API.Modules.Utils.QAngle angle)
    {
        return float.IsNaN(angle.X) || float.IsNaN(angle.Y) || float.IsNaN(angle.Z);
    }
    public static void Fix(this CounterStrikeSharp.API.Modules.Utils.QAngle angle)
    {
        angle.FixInfinity();
        angle.FixNaN();
        angle.Clamp();
    }

    private static void FixInfinity(this CounterStrikeSharp.API.Modules.Utils.QAngle angle)
    {
        if (!float.IsFinite(angle.X))
            angle.X = 0;
        if (!float.IsFinite(angle.Y))
            angle.Y = 0;
        if (!float.IsFinite(angle.Z))
            angle.Z = 0;
    }

    private static void FixNaN(this CounterStrikeSharp.API.Modules.Utils.QAngle angle)
    {
        if (float.IsNaN(angle.X))
            angle.X = 0;
        if (float.IsNaN(angle.Y))
            angle.Y = 0;
        if (float.IsNaN(angle.Z))
            angle.Z = 0;
    }

    private static void Clamp(this CounterStrikeSharp.API.Modules.Utils.QAngle angle)
    {
        angle.X = Math.Clamp(angle.X, -89.0f, 89.0f);
        angle.Y = Math.Clamp(angle.Y, -180.0f, 180.0f);
        angle.Z = 0;
    }

    private static bool IsReasonable(this CounterStrikeSharp.API.Modules.Utils.QAngle q )
    {
        const float r = 360.0f * 1000.0f;
        return
            q.X is > -r and < r &&
            q.Y is > -r and < r &&
            q.Z is > -r and < r;
    }
}