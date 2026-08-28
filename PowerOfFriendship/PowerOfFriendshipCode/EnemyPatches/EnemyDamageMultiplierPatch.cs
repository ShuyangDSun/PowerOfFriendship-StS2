using HarmonyLib;

using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;

namespace PowerOfFriendship.PowerOfFriendshipCode.EnemyPatches;

[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyDamage))]
internal class EnemyDamageMultiplierPatch
{
    [HarmonyPrefix]
    private static void Prefix(ref decimal __result, Creature? dealer)
    {
        if (dealer is not null && dealer.IsMonster)
        {
            __result *= PowerOfFriendship.TotalPlayers;
        }
    }
}