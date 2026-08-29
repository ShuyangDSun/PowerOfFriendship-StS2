using HarmonyLib;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

using PowerOfFriendship.PowerOfFriendshipCode.PlayerPatches;

namespace PowerOfFriendship.PowerOfFriendshipCode.EnemyPatches;

/// <summary>
/// Combat damage (dealer.IsMonster) is already shared across players elsewhere. This patch covers the other
/// source of HP loss: non-combat event/environmental damage, which the game always calls with a null dealer
/// (see e.g. SlipperyBridge, TrashHeap, StoneOfAllTime) and only against the one player interacting with the
/// event. Patching the lowest-level Damage overload means every higher-level overload (decimal or DamageVar
/// based) funnels through here regardless of which one the event code called.
/// </summary>
[HarmonyPatch(
    typeof(CreatureCmd),
    nameof(CreatureCmd.Damage),
    typeof(PlayerChoiceContext),
    typeof(IEnumerable<Creature>),
    typeof(decimal),
    typeof(ValueProp),
    typeof(Creature),
    typeof(CardModel))
]
internal class EventDamageSyncPatch
{
    [HarmonyPostfix]
    private static void Postfix(
        ref Task<IEnumerable<DamageResult>> __result,
        PlayerChoiceContext choiceContext,
        IEnumerable<Creature> targets,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        // Combat damage always has a dealer (the attacking monster); that path is synced separately.
        if (dealer != null)
        {
            return;
        }

        // Event damage is always dealt to exactly one creature. Bail if that assumption ever breaks
        // rather than guessing which target to sync from.
        var target = targets.SingleOrDefault();
        if (target is not { IsPlayer: true })
        {
            return;
        }

        __result = PlayerSync.ApplyEffectToPlayers(__result, target, ApplyDamage);
        return;

        Task<IEnumerable<DamageResult>> ApplyDamage(Creature otherTarget)
        {
            return CreatureCmd.Damage(choiceContext, otherTarget, amount, props, dealer, cardSource);
        }
    }
}
