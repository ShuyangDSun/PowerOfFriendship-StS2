using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;

namespace PowerOfFriendship.PowerOfFriendshipCode;

[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyDamage))]
public class EnemyDamageMultiplierPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref decimal __result, Creature? dealer)
    {
        if (dealer is not null && dealer.IsMonster)
        {
            __result *= MainFile.TotalPlayers;
        }
    }
}