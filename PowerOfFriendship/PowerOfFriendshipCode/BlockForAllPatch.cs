using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.ValueProps;

namespace PowerOfFriendship.PowerOfFriendshipCode;


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
    private static bool _synchronizing;
    
    [HarmonyPostfix]
    private static void Postfix(Creature creature, decimal amount, ValueProp props, CardPlay cardPlay, bool fast)
    {
        // don't block for none players (monsters, bosses, osty, etc...) and also prevent chain reaction blocking
        if (_synchronizing || !creature.IsPlayer)
        {
            return;
        }

        try
        {
            _synchronizing = true;

            if (creature.CombatState?.Players is null)
            {
                return;
            }
            
            foreach (var player in creature.CombatState.Players)
            {
                // don't block if player is yourself
                if (player.Creature != creature)
                {
                    CreatureCmd.GainBlock(player.Creature, amount, props, cardPlay, fast);
                }
            }
        }
        finally
        {
            _synchronizing = false;
        }
    }
}