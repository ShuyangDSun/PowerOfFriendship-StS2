using BaseLib.Config;

namespace PowerOfFriendship.PowerOfFriendshipCode;

internal sealed class ModConfig : SimpleModConfig
{
    // false combines health bar, true averages health bar
    public static bool AverageHealthBar { get; set; } = false;
}