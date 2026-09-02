using HarmonyLib;

using MegaCrit.Sts2.Core.Models.Powers;

namespace PowerOfFriendship.PowerOfFriendshipCode.PowerPatches;

[HarmonyPatch(typeof(ThornsPower), nameof(ThornsPower.BeforeDamageReceived))]
internal class ThornsPowerPatch
{
    private static readonly AsyncLocal<int> ResolvingThornsDamageDepth = new();
    internal static bool ResolvingThornsDamage => ResolvingThornsDamageDepth.Value > 0;

    [HarmonyPrefix]
    private static void Prefix()
    {
        ResolvingThornsDamageDepth.Value++;
    }

    [HarmonyPostfix]
    private static void Postfix()
    {
        ResolvingThornsDamageDepth.Value--;
    }
}