using HarmonyLib;

using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;

namespace PowerOfFriendship.PowerOfFriendshipCode.EnemyPatches;

[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyDamage))]
internal class EnemyDamageMultiplierPatch
{
    [HarmonyPrefix]
    private static void Prefix(ref decimal damage, Creature? dealer)
    {
        if (dealer?.IsMonster != true)
        {
            return;
        }
        
        damage *= PowerOfFriendship.TotalPlayers;
    }
}