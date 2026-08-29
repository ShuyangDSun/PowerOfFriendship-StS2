using HarmonyLib;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;

namespace PowerOfFriendship.PowerOfFriendshipCode.EnemyPatches;

[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyDamage))]
internal class EnemyDamageMultiplierPatch
{
    [HarmonyPrefix]
    private static void Prefix(ref decimal damage, Creature? dealer, Creature? target)
    {
        if (((target?.IsPlayer ?? false ) || (target?.IsPet ?? false)) && (dealer?.IsMonster ?? false))
        {
            damage *= PowerOfFriendship.TotalPlayers;
        }

        return;
    }
}