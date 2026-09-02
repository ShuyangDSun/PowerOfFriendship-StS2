using HarmonyLib;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;

using PowerOfFriendship.PowerOfFriendshipCode.Utils;

namespace PowerOfFriendship.PowerOfFriendshipCode.PowerPatches;

[HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Decrement))]
internal class DecrementPatch
{
    [HarmonyPrefix]
    private static void Prefix(PowerModel power)
    {
        SuppressSharing.StartSuppressingPower();
    }

    [HarmonyPostfix]
    private static void Postfix(PowerModel power)
    {
        SuppressSharing.StopSuppressingPower();
    }
}