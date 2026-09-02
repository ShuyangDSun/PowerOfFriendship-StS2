using HarmonyLib;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

using PowerOfFriendship.PowerOfFriendshipCode.Utils;

namespace PowerOfFriendship.PowerOfFriendshipCode.PowerPatches;

/*
 * This needs 2 patches because the Hierarchy of Apply goes:
 * Apply<T>(targets) ─> Apply<T>(target) ─> Apply(PowerModel, target) ─> ModifyAmount
 *                               └──> ModifyAmount
 * So we must patch both Apply and ModifyAmount
 * However since Apply(PowerModel, target) can also call ModifyAmount, we want to check for the case where
 * Apply(PowerModel, target) calls ModifyAmount and avoid doing anything if that is the case.
 *
 * Note: SandpitPower logic is a bit weird since the owner of the power is The Insatiable however The Insatiable
 *  also has its own count of the SandpitPower for each player
 */

[HarmonyPatch(
    typeof(PowerCmd),
    nameof(PowerCmd.Apply),
    typeof(PlayerChoiceContext),
    typeof(PowerModel),
    typeof(Creature),
    typeof(decimal),
    typeof(Creature),
    typeof(CardModel),
    typeof(bool))
]
internal class PowerApplyPatch
{
    [HarmonyPrefix]
    private static void Prefix(
        PowerModel power,
        Creature target,
        ref decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        // Skip if going to ModifyAmount
        PowerModel? instanceForStacking = PowerCmd.FindExistingInstanceForStacking(power, target, applier);
        if (instanceForStacking != null)
        {
            return;
        }

        PowerPatch.Prefix(power, target, ref amount, applier, cardSource);
    }

    [HarmonyPostfix]
    private static void Postfix(
        ref Task __result,
        PlayerChoiceContext choiceContext,
        PowerModel power,
        Creature target,
        decimal amount,
        Creature? applier,
        CardModel? cardSource,
        bool silent = false)
    {
        PowerPatch.Postfix(ref __result, choiceContext, power, target, amount, applier, cardSource, silent);
    }
}

[HarmonyPatch(
    typeof(PowerCmd),
    nameof(PowerCmd.ModifyAmount))
]
internal class PowerModifyAmountPatch
{
    [HarmonyPrefix]
    private static void Prefix(
        PowerModel power,
        ref decimal offset,
        Creature? applier,
        CardModel? cardSource)
    {
        Creature target = power.Owner;
        PowerPatch.Prefix(power, target, ref offset, applier, cardSource);
    }

    [HarmonyPostfix]
    private static void Postfix(
        ref Task<int> __result,
        PlayerChoiceContext choiceContext,
        PowerModel power,
        Decimal offset,
        Creature? applier,
        CardModel? cardSource,
        bool silent = false)
    {
        Creature target = power.Owner;
        PowerPatch.Postfix(ref __result, choiceContext, power, target, offset, applier, cardSource, silent);
    }
}


internal static class PowerPatch
{
    private static readonly HashSet<Type> FromCard = [
        typeof(DexterityPower),
        typeof(FrailPower),
    ];
    private static readonly HashSet<Type> EnemyToEnemy = [
        typeof(StrengthPower),
        typeof(VigorPower),
        typeof(RitualPower),
        typeof(SandpitPower)
    ];
    private static readonly HashSet<Type> PlayerToPlayer = [
        typeof(DoomPower),
    ];
    private static readonly HashSet<Type> EnemyToPlayer = [];
    private static readonly HashSet<Type> PlayerToEnemy = [];

    private static readonly HashSet<Type> ShouldNTimes = [
        typeof(StrengthPower),
        typeof(VigorPower),
        // typeof(RitualPower),
        // typeof(SuckPower),
        typeof(TerritorialPower),
        typeof(SteamEruptionPower),
        // typeof(CrabRagePower),
        // typeof(EnragePower),
        // typeof(PainfulStabsPower),
        // typeof(SandpitPower)
    ];
    private static readonly HashSet<Type> ShouldBeShared = [
        // typeof(NeurosurgePower),
        typeof(DoomPower),
        typeof(FrailPower),
        typeof(BarricadePower),
        typeof(BufferPower),
        typeof(ColossusPower),
        typeof(IntangiblePower),
        typeof(BlurPower),
        typeof(TaintedPower),
        // typeof(DisintegrationPower),
        // typeof(SandpitPower)
    ];

    internal static void Prefix(
        PowerModel power,
        Creature target,
        ref decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        // if powerType is SandpitPower, handle differently
        if (power is SandpitPower && cardSource is null)
        {
            amount *= PowerOfFriendship.TotalPlayers;
            return;
        }
        
        // All other powers
        Type powerType = power.GetType();

        if (!ShouldModifyPower(powerType, target, applier, cardSource))
        {
            return;
        }

        if (ShouldNTimes.Contains(powerType))
        {
            amount *= PowerOfFriendship.TotalPlayers;
        }
    }

    internal static void Postfix(
        ref Task __result,
        PlayerChoiceContext choiceContext,
        PowerModel power,
        Creature target,
        decimal amount,
        Creature? applier,
        CardModel? cardSource,
        bool silent = false)
    {
        Type powerType = power.GetType();

        if (!ShouldModifyPower(powerType, target, applier, cardSource))
        {
            return;
        }

        if (ShouldBeShared.Contains(powerType) && !SuppressSharing.SuppressPower)
        {
            // if powerType is SandpitPower, handle differently
            // if (power is SandpitPower && applier?.IsPlayer == true)
            // {
            //     __result = PlayerSync.ApplyEffectToPlayers(__result, applier, ApplySandPitPowerForOthersFunction);
            //     return;
            // }
            
            __result = PlayerSync.ApplyEffectToPlayers(__result, target, ApplyToOtherPlayersFunction);
        }

        return;

        Task ApplyToOtherPlayersFunction(Creature player) =>
            PowerCmd.Apply(choiceContext, (PowerModel) power.ClonePreservingMutability(), player, amount, applier, cardSource, silent);

        Task ApplySandPitPowerForOthersFunction(Creature player)
        {
            Creature? theInsatiable = player.CombatState?.Enemies.FirstOrDefault(c => c.HasPower<SandpitPower>());
            return theInsatiable is null ? 
                Task.CompletedTask :
                PowerCmd.Apply(choiceContext, power, target, amount, applier, cardSource);
        }
    }
    internal static void Postfix<T>(
        ref Task<T?> __result,
        PlayerChoiceContext choiceContext,
        PowerModel power,
        Creature target,
        decimal amount,
        Creature? applier,
        CardModel? cardSource,
        bool silent = false)
    {
        Type powerType = power.GetType();

        if (!ShouldModifyPower(powerType, target, applier, cardSource))
        {
            return;
        }

        if (ShouldBeShared.Contains(powerType) && !SuppressSharing.SuppressPower)
        {
            // if powerType is SandpitPower, handle differently
            // if (power is SandpitPower && applier?.IsPlayer == true)
            // {
            //     PowerOfFriendship.Logger.Info("HANDLING SHARING SANDPIT POWER");
            //     __result = PlayerSync.ApplyEffectToPlayers(__result, applier, ApplySandPitPowerForOthersFunction);
            //     return;
            // }
            
            __result = PlayerSync.ApplyEffectToPlayers(__result, target, ApplyToOtherPlayersFunction);
        }

        return;

        Task ApplyToOtherPlayersFunction(Creature player) =>
            PowerCmd.Apply(choiceContext, (PowerModel) power.ClonePreservingMutability(), player, amount, applier, cardSource, silent);
        
        Task ApplySandPitPowerForOthersFunction(Creature player)
        {
            Creature? theInsatiable = player.CombatState?.Enemies.FirstOrDefault(c => c.HasPower<SandpitPower>());
            PowerOfFriendship.Logger.Info("SHARING SANDPIT POWER");
            PowerOfFriendship.Logger.Info((theInsatiable is null).ToString());
            return theInsatiable is null ?
                Task.CompletedTask :
                PowerCmd.Apply(choiceContext, power, target, amount, applier, cardSource);
        }
    }

    private static bool ShouldModifyPower(
        Type powerType,
        Creature target,
        Creature? applier,
        CardModel? cardSource)
    {
        // If power should only be modified when from a card and onto a player but it isn't, skip
        if (FromCard.Contains(powerType) && (cardSource == null || !target.IsPlayer))
        {
            return false;
        }

        // If power should only be modified when players apply it to themselves but it isn't, skip
        if (PlayerToPlayer.Contains(powerType) && (!target.IsPlayer || applier?.IsPlayer != true))
        {
            return false;
        }

        // If power should only be modified when enemies (or something else) applies it to players but it isn't, skip
        if (EnemyToPlayer.Contains(powerType) && (!target.IsPlayer || applier?.IsPlayer == true))
        {
            return false;
        }

        // if power should only be modified when players apply it to enemies but it isn't, skip (no use case currently)
        if (PlayerToEnemy.Contains(powerType) && (!target.IsMonster || target.IsPet || applier?.IsPlayer != true))
        {
            return false;
        }

        // if power should only be modified when non-players apply it to enemies but it isn't, skip
        if (EnemyToEnemy.Contains(powerType) && (!target.IsMonster || target.IsPet || applier?.IsPlayer == true))
        {
            return false;
        }

        return true;
    }
}