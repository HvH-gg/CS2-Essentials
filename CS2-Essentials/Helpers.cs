using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace hvhgg_essentials;

public static class Helpers
{
    private static readonly Dictionary<string, char> PredefinedColors = new()
    {
        { "{Default}", ChatColors.Default },
        { "{White}", ChatColors.White },
        { "{DarkRed}", ChatColors.DarkRed },
        { "{LightPurple}", ChatColors.LightPurple },
        { "{Green}", ChatColors.Green },
        { "{Olive}", ChatColors.Olive },
        { "{Lime}", ChatColors.Lime },
        { "{Red}", ChatColors.Red },
        { "{Grey}", ChatColors.Grey },
        { "{Gold}", ChatColors.Gold },
        { "{Silver}", ChatColors.Silver },
        { "{Blue}", ChatColors.Blue },
        { "{DarkBlue}", ChatColors.DarkBlue },
        { "{Magenta}", ChatColors.Magenta },
        { "{LightRed}", ChatColors.LightRed },
        { "{Orange}", ChatColors.Orange },
    };
    
    private static readonly Dictionary<string, string> Constants = new()
    {
        { "{HvHgg}", $"{ChatColors.Red}Hv{ChatColors.DarkRed}H{ChatColors.Default}.gg" },
    };

    public static string FormatMessage(string message)
    {
        message = FormatColorInMessage(message);
        message = FormatConstantsInMessage(message);

        return message;
    }
    
    private static string FormatColorInMessage(string message)
    {
        foreach (var color in PredefinedColors)
            message = message.Replace(color.Key, $"{color.Value}");

        return message;
    }
    
    private static string FormatConstantsInMessage(string message)
    {
        foreach (var constant in Constants)
            message = message.Replace(constant.Key, $"{constant.Value}");

        return message;
    }
}