using HarmonyLib;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace PowerOfFriendship.PowerOfFriendshipCode.RelicPatches;

[HarmonyPatch(typeof(BeatingRemnant), nameof(BeatingRemnant.ModifyHpLostAfterOsty))]
internal static class BeatingRemnantPatch
{
    [HarmonyPrefix]
    private static bool ModifyHpLostAfterOstyPrefix(
        ref decimal __result,
        BeatingRemnant __instance,
        Creature target,
        decimal amount)
    {
        if (!target.IsPlayer)
        {
            return true;
        }
        
        // get private variable
        var damageReceivedThisTurn = Traverse.Create(__instance)
            .Property("DamageReceivedThisTurn")
            .GetValue<decimal>();
        
        __result = !CombatManager.Instance.IsInProgress
            ? amount
            : Math.Min(amount, __instance.DynamicVars["MaxHpLoss"].BaseValue * PowerOfFriendship.TotalPlayers - damageReceivedThisTurn);
        return false;
    }
}