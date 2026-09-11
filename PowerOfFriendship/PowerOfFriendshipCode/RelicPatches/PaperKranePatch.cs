using HarmonyLib;

using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace PowerOfFriendship.PowerOfFriendshipCode.RelicPatches;

[HarmonyPatch(typeof(PaperKrane), nameof(PaperKrane.ModifyWeakMultiplier))]
internal class PaperKranePatch
{
    [HarmonyPrefix]
    private static bool Prefix(
        ref decimal __result,
        Creature target,
        decimal amount,
        ValueProp props)
    {
        if (!target.IsPlayer)
        {
            return true;
        }

        __result = !props.IsPoweredAttack() ? amount : amount - 0.15M;
        return false;
    }
}

// Patch Weak so that PaperKrane applies it's effect to all players
[HarmonyPatch(typeof(WeakPower), nameof(WeakPower.ModifyDamageMultiplicative))]
internal static class WeakPowerPatch
{
    [HarmonyPostfix]
    private static void Postfix(
        ref decimal __result,
        WeakPower __instance,
        Creature? target,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        // checks to see if we should apply PaperKrane effect
        if (dealer != __instance.Owner || !props.IsPoweredAttack() || target?.IsPlayer != true)
        {
            return;
        }
        // Debilitate and PaperKrane modifiers can't happen at the same time, but just in case, this preserves the order
        DebilitatePower? power = dealer.GetPower<DebilitatePower>();
        if (power != null)
        {
            return;
        }
        
        decimal amount1 = __instance.DynamicVars["DamageDecrease"].BaseValue;
        
        PaperKrane? relic = PartyRelics.GetRelic<PaperKrane>();
        if (relic != null)
            __result = relic.ModifyWeakMultiplier(target, amount1, props, dealer, cardSource);
    }
}