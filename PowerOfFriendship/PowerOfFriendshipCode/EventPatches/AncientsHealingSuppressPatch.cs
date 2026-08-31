using HarmonyLib;

using MegaCrit.Sts2.Core.Models;

namespace PowerOfFriendship.PowerOfFriendshipCode.EventPatches;

[HarmonyPatch(typeof(AncientEventModel), "BeforeEventStarted", typeof(bool))]
internal class AncientsHealingSuppressPatch
{
    internal static bool SuppressHealing;

    [HarmonyPrefix]
    private static void Prefix(out bool __state)
    {
        __state = SuppressHealing;
        SuppressHealing = true;
    }

    [HarmonyPostfix]
    private static void Postfix(Task __result, bool __state)
    {
        __result.GetAwaiter().GetResult();
        SuppressHealing = __state;
    }
}