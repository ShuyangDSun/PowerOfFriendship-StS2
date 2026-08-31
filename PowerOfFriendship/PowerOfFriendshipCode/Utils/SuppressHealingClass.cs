namespace PowerOfFriendship.PowerOfFriendshipCode.Utils;

internal class SuppressHealingClass
{
    private static readonly AsyncLocal<int> SuppressHealingDepth = new();
    internal static bool SuppressHealing { get { return SuppressHealingDepth.Value > 0; } }
    internal static void StartSuppressionHealing() => SuppressHealingDepth.Value++;
    internal static void StopSuppressionHealing() =>
        SuppressHealingDepth.Value = Math.Max(SuppressHealingDepth.Value - 1, 0);
}
