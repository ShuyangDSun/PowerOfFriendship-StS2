namespace PowerOfFriendship.PowerOfFriendshipCode.Utils;

internal class SuppressHealingClass
{
    private static readonly AsyncLocal<int> SuppressHealingDepth = new();
    internal static bool SuppressHealing { get { return SuppressHealingDepth.Value > 0; } }
    internal static void StartSuppressionHealing() => SuppressHealingDepth.Value++;
    internal static void StopSuppressionHealing() => SuppressHealingDepth.Value--;
    internal static async Task StopSuppressionHealingAfterAsync(Task task)
    {
        await task;
        StopSuppressionHealing();
    }

    internal static async Task<T> StopSuppressionHealingAfterAsync<T>(Task<T> task)
    {
        T result = await task;
        StopSuppressionHealing();
        return result;
    }
}