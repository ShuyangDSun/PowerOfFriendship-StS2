using HarmonyLib;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;

namespace PowerOfFriendship.PowerOfFriendshipCode.RelicPatches;

[HarmonyPatch(typeof(DiamondDiadem))]
internal static class DiamondDiademPatch
{
    [HarmonyPatch(nameof(DiamondDiadem.AfterCardPlayed))]
    [HarmonyPrefix]
    private static bool AfterCardPlayedPrefix(
        ref Task __result,
        DiamondDiadem __instance)
    {
        if (!CombatManager.Instance.IsInProgress)
        {
            __result = Task.CompletedTask;
            return false;
        }
        
        ++__instance.CardsPlayedThisTurn;
        
        // run private function
        AccessTools.Method(typeof(DiamondDiadem), "RefreshCounter")
            .Invoke(__instance, null);
        
        __result = Task.CompletedTask;
        return false;
    }

    [HarmonyPatch(nameof(DiamondDiadem.BeforeSideTurnEnd))]
    [HarmonyPrefix]
    private static bool BeforeSideTurnEndPrefix(
        ref Task __result,
        DiamondDiadem __instance,
        PlayerChoiceContext choiceContext,
        IEnumerable<Creature> participants)
    {
        __result = BeforeSideTurnEndPrefixWrapper(__instance, choiceContext, participants);
        return false;
    }

    private static async Task BeforeSideTurnEndPrefixWrapper(
        DiamondDiadem diamondDiadem,
        PlayerChoiceContext choiceContext,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(diamondDiadem.Owner.Creature))
        {
            return;
        }
        
        if (diamondDiadem.CardsPlayedThisTurn <= diamondDiadem.DynamicVars["CardThreshold"].BaseValue * PowerOfFriendship.TotalPlayers)
        {
            diamondDiadem.Flash();
            
            // applying power to all is handled in PowerPatch
            await PowerCmd.Apply<DiamondDiademPower>(choiceContext, diamondDiadem.Owner.Creature, 1M, diamondDiadem.Owner.Creature, null);
        }
        diamondDiadem.CardsPlayedThisTurn = 0;
        
        // run private function
        AccessTools.Method(typeof(DiamondDiadem), "RefreshCounter")
            .Invoke(diamondDiadem, null);
    }
}