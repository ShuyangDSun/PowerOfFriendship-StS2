using HarmonyLib;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

using PowerOfFriendship.PowerOfFriendshipCode.Utils;

namespace PowerOfFriendship.PowerOfFriendshipCode.RelicPatches;

/*
 * Summary of how this code works: when players take damage, choose a LizardTail or FairyInABottle from the party to use
 *
 * LizardTail:
 * If we determine that the LizardTail has been consumed, then we can set the WasUsed property to true and remove it
 * from party relics
 *
 * Return true for ShouldDie if the LizardTail being used is not the one we chose. Since every player will loop through
 * every LizardTail, at least one of them will return false unless all of them have been used.
 * Since we want to use FairyInABottle as a revive first, return true also if s_bottle is not null
 * 
 * Make sure to set WasUsed to false after using it once so that other players can use it before we set it to true after
 *
 * 
 * FairyInABottle:
 * If we determine that the FairyInABottle has been consumed, then we remove it from players inventory (we suppress the
 * removal on use while trying to party heal)
 * 
 * Return true for ShouldDie if the FairyInABottle being used is not the one we chose. Since every player will loop
 * through every FairyInABottle, at least one of them will return false unless all of them have been used.
 *
 * Make sure to suppress the RemoveAfterUse function while trying to party heal, otherwise it removes the bottle from
 * the combat hooks so other players can't revive.
 */
internal static class PartyRevive
{
    private static FairyInABottle? s_bottle;
    private static LizardTail? s_tail;
    private static bool s_bottleTriggered;
    private static bool s_tailTriggered;
    
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
    private static class DamagePatch
    {
        [HarmonyPrefix]
        private static void Prefix(
            out bool __state)
        {
            __state = false;
            
            // if s_tail is not null, then this is a nested call. So don't do anything.
            if (s_bottle is not null || s_tail is not null)
            {
                return;
            }

            var players = PowerOfFriendship.Players;
            if (players.Count == 0)
            {
                return;
            }

            var bottle = 
                players
                    .SelectMany(player => player.Potions)
                    .OfType<FairyInABottle>()
                    .FirstOrDefault();
            
            var tail = PartyRelics.GetRelic<LizardTail>();
            
            if (bottle is null && tail is null)
            {
                return;
            }

            s_tail = tail;
            s_bottle = bottle;
            s_tailTriggered = false;
            __state = true;
        }

        [HarmonyPostfix]
        private static void Postfix(ref Task __result, bool __state)
        {
            // Since CreatureCmd.Damage can call itself multiple times, __state makes sure that this isn't run multiple times
            if (__state)
            {
                __result = FinishPartyRevive(__result);
            }
        }

        private static async Task FinishPartyRevive(Task killTask)
        {
            try
            {
                await killTask;
            }
            finally
            {
                if (s_bottleTriggered && s_bottle is not null)
                {
                    // flag must be set to false for RemoveBeforeUse() to work
                    s_bottleTriggered = false;
                    s_bottle.RemoveBeforeUse();
                }
                
                if (s_tailTriggered && s_tail is not null)
                {
                    s_tail.WasUsed = true;
                    PartyRelics.Remove(s_tail);
                }
                s_bottle = null;
                s_tail = null;
                s_bottleTriggered = false;
                s_tailTriggered = false;
            }
        }
    }
    
    // ------------------------- FairyInABottle -------------------------
    [HarmonyPatch(typeof(PotionModel), nameof(PotionModel.RemoveBeforeUse))]
    private static class PotionModelPatch
    {
        [HarmonyPrefix]
        private static bool RemoveBeforeUsePrefix(PotionModel __instance)
        {
            // if group heal is in progress, don't remove the bottle until after healing is done for all party members
            return __instance is not FairyInABottle || !s_bottleTriggered;
        }
    }

    [HarmonyPatch(typeof(FairyInABottle))]
    private static class FairyInABottlePatch
    {
        [HarmonyPatch(nameof(FairyInABottle.ShouldDie))]
        [HarmonyPrefix]
        private static bool ShouldDiePrefix(FairyInABottle __instance, ref bool __result, Creature creature)
        {
            // if no bottle is found
            if (s_bottle is null)
            {
                return true;
            }
            
            // Prevents a second FairyInABottle from being used
            __result = __instance != s_bottle;
            return false;
        }
        
        [HarmonyPatch(nameof(FairyInABottle.AfterPreventingDeath))]
        [HarmonyPrefix]
        private static void AfterPreventingDeathPrefix()
        {
            SuppressSharing.StartSuppressingHealing();
            
            if (!s_bottleTriggered)
            {
                s_bottleTriggered = true;
            }
        }
        
        [HarmonyPatch(nameof(FairyInABottle.AfterPreventingDeath))]
        [HarmonyPostfix]
        private static void AfterPreventingDeathPostfix()
        {
            SuppressSharing.StopSuppressingHealing();
        }
    }

    // ------------------------- LizardTail -------------------------
    [HarmonyPatch(typeof(LizardTail))]
    private static class LizardTailPatch
    {
        [HarmonyPatch(nameof(LizardTail.ShouldDieLate))]
        [HarmonyPrefix]
        private static bool ShouldDieLatePrefix(LizardTail __instance, ref bool __result)
        {
            // let fairy in a bottle group revive
            if (s_bottle is not null)
            {
                __result = true;
                return false;
            }
            
            // if no tail is found
            if (s_tail is null)
            {
                return true;
            }
            
            // Prevents a second LizardTail from being used
            __result = __instance != s_tail || __instance.WasUsed;
            return false;
        }
        
        [HarmonyPatch(nameof(LizardTail.AfterPreventingDeath))]
        [HarmonyPrefix]
        private static void AfterPreventingDeathPrefix()
        {
            SuppressSharing.StartSuppressingHealing();
            
            if (!s_tailTriggered)
            {
                s_tailTriggered = true;
            }
        }
        
        [HarmonyPatch(nameof(LizardTail.AfterPreventingDeath))]
        [HarmonyPostfix]
        private static void AfterPreventingDeathPostfix(LizardTail __instance)
        {
            // keep it usable for the rest of party members and then end it later
            __instance.WasUsed = false;
            SuppressSharing.StopSuppressingHealing();
        }
    }
}
