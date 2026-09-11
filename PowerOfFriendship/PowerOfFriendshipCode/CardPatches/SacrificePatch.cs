using HarmonyLib;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;

using PowerOfFriendship.PowerOfFriendshipCode.Utils;

namespace PowerOfFriendship.PowerOfFriendshipCode.CardPatches;

[HarmonyPatch(typeof(Sacrifice), "OnPlay")]
internal static class SacrificePatch
{
    private static void Postfix(ref Task __result, Sacrifice __instance)
    {
        if (__instance.Owner.IsOstyMissing)
        {
            return;
        }
        
        __result = PlayerSync.ApplyEffectToPlayers(__result, __instance.Owner.Creature, KillOstyForAll);
        return;

        Task KillOstyForAll(Creature target)
        {
            if (target.Player is null || target.Player.IsOstyMissing)
            {
                return Task.CompletedTask;
            }
            
            return CreatureCmd.Kill(target.Player.Osty!);
        }
    }
}