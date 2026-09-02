using MegaCrit.Sts2.Core.Entities.Creatures;

namespace PowerOfFriendship.PowerOfFriendshipCode.Utils;

/*
 * PlayerSync
 *
 * Private variable:
 *  Synchronizing: used to prevent recursive chains in current Async flow.
 * 
 * Methods:
 *  ApplyEffectToPlayers is a function that applies a function/command to all players
 *
 * How to use:
 *  When Postfix patching an async function, the Postfix function runs before the async function actually
 *  resolves. In the postfix function, obtain the Task that it returns as a ref and replace the Task with
 *  ApplyEffectToPlayers.
 *
 *  The ref now points to a new Task and will await that before continuing with other in-game events
 */

internal static class PlayerSync
{
    private static readonly AsyncLocal<bool> Synchronizing = new();

    internal static async Task ApplyEffectToPlayers(Task originalCommand, Creature target, Func<Creature, Task> applyToTarget)
    {
        // finish original command
        await originalCommand;

        // apply to other players if not currently trying to sync effects and affected creature is a player
        if (!Synchronizing.Value && target.IsPlayer)
        {
            await ApplyToPlayers(originalCommand, target, applyToTarget);
        }
    }

    internal static async Task<T> ApplyEffectToPlayers<T>(Task<T> originalCommand, Creature target,
        Func<Creature, Task> applyToTarget)
    {
        // finish original command
        var result = await originalCommand;

        // apply to other players if not currently trying to sync effects and affected creature is a player
        if (!Synchronizing.Value && target.IsPlayer)
        {
            await ApplyToPlayers(originalCommand, target, applyToTarget);
        }
        return result;
    }

    private static async Task ApplyToPlayers(Task originalCommand, Creature target, Func<Creature, Task> applyToTarget)
    {
        try
        {
            // prevents recursive calling (Example: GainBlock > all players GainBlock > every player evokes another Postfix...)
            Synchronizing.Value = true;

            if (target.Player?.RunState.Players is null)
            {
                return;
            }

            await originalCommand;

            foreach (var player in target.Player.RunState.Players)
            {
                if (player.Creature != target)
                {
                    await applyToTarget(player.Creature);
                }
            }
        }
        finally
        {
            Synchronizing.Value = false;
        }
    }
}