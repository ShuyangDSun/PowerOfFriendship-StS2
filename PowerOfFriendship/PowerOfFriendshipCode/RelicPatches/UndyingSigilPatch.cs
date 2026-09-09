using HarmonyLib;

using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace PowerOfFriendship.PowerOfFriendshipCode.RelicPatches;

[HarmonyPatch(typeof(UndyingSigil), nameof(UndyingSigil.ModifyDamageMultiplicative))]
internal class UndyingSigilPatch
{
    [HarmonyPrefix]
    private static bool Prefix(
        ref decimal __result,
        UndyingSigil __instance,
        Creature? target,
        ValueProp props,
        Creature? dealer)
    {
        if (target?.IsPlayer != true)
        {
            return true;
        }
        
        __result = dealer == null || !props.IsPoweredAttack() || dealer.IsPlayer
                   || dealer.CurrentHp > dealer.GetPowerAmount<DoomPower>()
            ? 1M
            : __instance.DynamicVars["DamageDecrease"].BaseValue;
        return false;
    }
}