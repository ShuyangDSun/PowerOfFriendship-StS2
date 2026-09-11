using HarmonyLib;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace PowerOfFriendship.PowerOfFriendshipCode.RelicPatches;

[HarmonyPatch(typeof(SturdyClamp))]
internal static class SturdyClampPatch
{
    [HarmonyPatch(nameof(SturdyClamp.ShouldClearBlock))]
    [HarmonyPostfix]
    private static void ShouldClearBlockPostfix(ref bool __result, Creature creature)
    {
        if (creature.IsPlayer)
        {
            __result = false;
        }
    }

    [HarmonyPatch(nameof(SturdyClamp.AfterPreventingBlockClear))]
    [HarmonyPrefix]
    private static bool AfterPreventingBlockClearPrefix(
        ref Task __result,
        SturdyClamp __instance,
        AbstractModel preventer,
        Creature creature)
    {
        if (__instance != preventer || !creature.IsPlayer)
            return true;
        
        __result = PreventBlock(__instance, creature);
        return false;
    }

    private static async Task PreventBlock(SturdyClamp sturdyClamp, Creature creature)
    {
        int block = creature.Block;
        if (block > 10)
            await CreatureCmd.LoseBlock(creature, block - 10);

        if (block > 0)
        {
            sturdyClamp.Flash();
        }
    }
}