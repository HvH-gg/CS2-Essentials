using System.Runtime.CompilerServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;

namespace hvhgg_essentials;

public class CUserCmd
{
    public CUserCmd(IntPtr pointer)
    {
        Handle = pointer;
    }

    private IntPtr Handle { get; set; }
    
    public unsafe QAngle? GetViewAngles()
    {
        if (Handle == IntPtr.Zero)
            return null;

        var baseCmd = Unsafe.Read<IntPtr>((void*)(Handle + 0x40));
        if (baseCmd == IntPtr.Zero)
            return null;

        var msgQAngle = Unsafe.Read<IntPtr>((void*)(baseCmd + 0x40));
        if (msgQAngle == IntPtr.Zero)
            return null;

        var viewAngles = new QAngle(msgQAngle + 0x18);
        
        return viewAngles.Handle == IntPtr.Zero ? null : viewAngles;
    }
}