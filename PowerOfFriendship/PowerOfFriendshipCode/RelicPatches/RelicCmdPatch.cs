using HarmonyLib;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace PowerOfFriendship.PowerOfFriendshipCode.RelicPatches;

/*
 * This Patch is used to keep track of the relics the party shares together
 */
[HarmonyPatch(typeof(RelicCmd))]
internal static class RelicObtainPatch
{
    [HarmonyPatch(nameof(RelicCmd.Obtain), typeof(RelicModel), typeof(Player), typeof(int))]
    [HarmonyPostfix]
    private static void ObtainPostfix(RelicModel relic)
    {
        PartyRelics.Add(relic);
    }

    [HarmonyPatch(nameof(RelicCmd.Remove))]
    [HarmonyPostfix]
    private static void RemovePostfix(RelicModel relic)
    {
        PartyRelics.Remove(relic);
    }

    [HarmonyPatch(nameof(RelicCmd.Melt))]
    [HarmonyPostfix]
    private static void MeltPostfix(RelicModel relic)
    {
        PartyRelics.Remove(relic);
    }
}