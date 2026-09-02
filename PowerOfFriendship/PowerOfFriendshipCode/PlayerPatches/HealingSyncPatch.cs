using HarmonyLib;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;

using PowerOfFriendship.PowerOfFriendshipCode.EventPatches;
using PowerOfFriendship.PowerOfFriendshipCode.Utils;

namespace PowerOfFriendship.PowerOfFriendshipCode.PlayerPatches;

[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Heal))]
public class HealingSyncPatch
{
    [HarmonyPostfix]
    private static void Postfix(ref Task __result, Creature creature, Decimal amount, bool playAnim)
    {
        if (SuppressSharing.SuppressHealing)
        {
            return;
        }

        __result = PlayerSync.ApplyEffectToPlayers(__result, creature, HealOtherPlayers);
        return;

        Task HealOtherPlayers(Creature target) => CreatureCmd.Heal(target, amount, playAnim);
    }
}