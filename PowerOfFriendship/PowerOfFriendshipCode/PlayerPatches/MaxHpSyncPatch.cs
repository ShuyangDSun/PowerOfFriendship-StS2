using HarmonyLib;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

using PowerOfFriendship.PowerOfFriendshipCode.Utils;

namespace PowerOfFriendship.PowerOfFriendshipCode.PlayerPatches;

[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.LoseMaxHp))]
internal class LoseMaxHpSyncPatch
{
    [HarmonyPostfix]
    private static void Postfix(
        ref Task __result,
        PlayerChoiceContext choiceContext,
        Creature creature,
        decimal amount,
        bool isFromCard)
    {
        __result = PlayerSync.ApplyEffectToPlayers(__result, creature, ApplyToTarget);
        return;

        Task ApplyToTarget(Creature target) => CreatureCmd.LoseMaxHp(choiceContext, target, amount, isFromCard);
    }
}

[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.GainMaxHp))]
internal class GainMaxHpSyncPatch
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        SuppressSharing.StartSuppressingHealing();
    }

    [HarmonyPostfix]
    private static void Postfix(
        ref Task __result,
        Creature creature,
        decimal amount)
    {
        __result = PlayerSync.ApplyEffectToPlayers(__result, creature, ApplyToTarget);
        return;

        Task ApplyToTarget(Creature target) => CreatureCmd.GainMaxHp(target, amount);
    }
}