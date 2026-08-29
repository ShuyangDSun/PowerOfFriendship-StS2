using HarmonyLib;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Runs;

namespace PowerOfFriendship.PowerOfFriendshipCode;

/// <summary> Sets all player health bars equal (either combined or averaged) and sets TotalPlayers variable </summary>
[HarmonyPatch(typeof(RunState), nameof(RunState.CreateForNewRun))]
internal class CreateNewRunPatch
{
    [HarmonyPostfix]
    private static void Postfix(RunState __result)
    {
        // set total players to use in other patches
        PowerOfFriendship.TotalPlayers = __result.Players.Count;
        var totalHealth = __result.Players.Sum(player => player.Creature.MaxHp);
        var currentHp = __result.Players.Sum(player => player.Creature.CurrentHp);
        
        foreach (var player in __result.Players)
        {
            CreatureCmd.SetCurrentHp(player.Creature, currentHp);
            CreatureCmd.SetMaxHp(player.Creature, totalHealth);
        }
    }
}

[HarmonyPatch(typeof(RunState), nameof(RunState.FromSerializable))]
internal class LoadRunPatch
{
    public static bool isDebug = false;
    [HarmonyPostfix]
    private static void Postfix(RunState __result)
    {
        PowerOfFriendship.TotalPlayers = __result.Players.Count;
        if (isDebug)
        {
            PowerOfFriendship.TotalPlayers = 4;
        }
    }
}