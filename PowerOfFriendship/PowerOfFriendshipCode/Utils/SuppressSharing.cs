namespace PowerOfFriendship.PowerOfFriendshipCode.Utils;

internal class SuppressSharing
{
    private static readonly AsyncLocal<int> SuppressHealingDepth = new();
    private static readonly AsyncLocal<int> SuppressPowerDepth = new();
    internal static bool SuppressHealing => SuppressHealingDepth.Value > 0;
    internal static bool SuppressPower => SuppressPowerDepth.Value > 0;
    internal static void StartSuppressingHealing() => SuppressHealingDepth.Value++;
    internal static void StartSuppressingPower() => SuppressPowerDepth.Value++;
    internal static void StopSuppressingHealing() =>
        SuppressHealingDepth.Value = Math.Max(SuppressHealingDepth.Value - 1, 0);
    internal static void StopSuppressingPower() =>
        SuppressPowerDepth.Value = Math.Max(SuppressPowerDepth.Value - 1, 0);
}
