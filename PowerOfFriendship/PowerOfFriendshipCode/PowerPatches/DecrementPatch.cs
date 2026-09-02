using HarmonyLib;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

using PowerOfFriendship.PowerOfFriendshipCode.Utils;

namespace PowerOfFriendship.PowerOfFriendshipCode.PowerPatches;

[HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Decrement))]
public class DecrementPatch
{
    [HarmonyPrefix]
    public static void Prefix(PowerModel power)
    {
        SuppressSharing.StartSuppressingPower();
    }

    [HarmonyPostfix]
    public static void Postfix(PowerModel power)
    {
        SuppressSharing.StopSuppressingPower();
    }
}