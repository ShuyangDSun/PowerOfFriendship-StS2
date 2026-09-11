using HarmonyLib;

using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace PowerOfFriendship.PowerOfFriendshipCode.RelicPatches;

[HarmonyPatch(typeof(TungstenRod))]
internal static class TungstenRodPatch
{
    [HarmonyPatch(nameof(TungstenRod.ModifyHpLostAfterOsty))]
    [HarmonyPrefix]
    private static bool Prefix(
        ref decimal __result,
        TungstenRod __instance,
        Creature target,
        decimal amount)
    {
        if (!target.IsPlayer)
        {
            return true;
        }
        __result = Math.Max(0M, amount - __instance.DynamicVars["HpLossReduction"].BaseValue);
        return false;
    }
}