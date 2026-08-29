using HarmonyLib;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

using PowerOfFriendship.PowerOfFriendshipCode.PlayerPatches;

namespace PowerOfFriendship.PowerOfFriendshipCode.EnemyPatches;

/// <summary>
/// Combat damage (dealer.IsMonster) is already shared across players elsewhere. This patch covers every other
/// source of HP loss against a player:
///  - non-combat event/environmental damage, which the game calls with a null dealer
///    (see e.g. SlipperyBridge, TrashHeap, StoneOfAllTime)
///  - self-damage cards/relics/potions, which pass a CardModel and resolve dealer to the casting
///    player's own creature (see e.g. Hemokinesis: CreatureCmd.Damage(choiceContext, Owner.Creature, amount, props, this))
/// Patching the lowest-level Damage overload means every higher-level overload (decimal, DamageVar, or
/// CardModel based) funnels through here regardless of which one the caller used.
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
        // Only skip actual monster-dealt combat damage; that path is synced separately. Everything else
        // that damages a player - a null dealer (events) or a player dealer (self-damage) or relic damage - is synced here.
        if (dealer is { IsMonster: true })
        {
            return;
        }

        // Event and self-damage sources are always dealt to exactly one creature. Bail if that
        // assumption ever breaks rather than guessing which target to sync from.
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
