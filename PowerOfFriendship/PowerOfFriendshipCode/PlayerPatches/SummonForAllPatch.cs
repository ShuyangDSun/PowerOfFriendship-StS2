
using HarmonyLib;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace PowerOfFriendship.PowerOfFriendshipCode.PlayerPatches;
[HarmonyPatch(
    typeof(OstyCmd),
    nameof(OstyCmd.Summon))
]
public class SummonForAllPatch
{
    [HarmonyPostfix]
    private static void Postfix(ref Task<decimal> __result, PlayerChoiceContext choiceContext, Player summoner, Decimal amount, AbstractModel source)
    {
        __result = PlayerSync.ApplyEffectToPlayers(__result, summoner.Creature, GainSummon);
        return;
        async Task<SummonResult> GainSummon(Creature target)
        {
             SummonResult summonResult = await OstyCmd.Summon(choiceContext, summoner, amount, source);
             return summonResult;
        }
    }
}