using HarmonyLib;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.ValueProps;

using PowerOfFriendship.PowerOfFriendshipCode.Utils;

namespace PowerOfFriendship.PowerOfFriendshipCode.PlayerPatches;


[HarmonyPatch(
    typeof(CreatureCmd),
    nameof(CreatureCmd.GainBlock),
    typeof(Creature),
    typeof(decimal),
    typeof(ValueProp),
    typeof(CardPlay),
    typeof(bool))
]
internal class BlockForAllPatch
{
    [HarmonyPostfix]
    private static void Postfix(
        ref Task<decimal> __result,
        Creature creature,
        decimal amount,
        ValueProp props,
        CardPlay? cardPlay,
        bool fast)
    {
        __result = PlayerSync.ApplyEffectToPlayers(__result, creature, GainBlock);
        return;

        Task<decimal> GainBlock(Creature target)
        {
            return CreatureCmd.GainBlock(target, amount, props, cardPlay, fast);
        }
    }
}