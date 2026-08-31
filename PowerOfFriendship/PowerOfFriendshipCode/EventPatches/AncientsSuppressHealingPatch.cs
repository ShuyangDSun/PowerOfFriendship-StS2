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
        SuppressHealingClass.StartSuppressionHealing();
    }

    [HarmonyPostfix]
    private static void Postfix(ref Task __result)
    {
        __result = SuppressHealingClass.StopSuppressionHealingAfterAsync(__result);
    }
}