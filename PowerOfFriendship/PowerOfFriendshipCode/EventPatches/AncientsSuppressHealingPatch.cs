using HarmonyLib;

using MegaCrit.Sts2.Core.Models;

using PowerOfFriendship.PowerOfFriendshipCode.Utils;

namespace PowerOfFriendship.PowerOfFriendshipCode.EventPatches;

[HarmonyPatch(typeof(AncientEventModel), "BeforeEventStarted", typeof(bool))]
internal class AncientsSuppressHealingPatch
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        SuppressSharing.StartSuppressingHealing();
    }

    [HarmonyPostfix]
    private static void Postfix()
    {
        SuppressSharing.StopSuppressingHealing();
    }
}