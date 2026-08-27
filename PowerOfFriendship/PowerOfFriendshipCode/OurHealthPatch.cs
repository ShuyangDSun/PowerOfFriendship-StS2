using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Runs;

namespace PowerOfFriendship.PowerOfFriendshipCode;

[HarmonyPatch(typeof(RunState), nameof(RunState.CreateForNewRun))]
public class OurHealthPatch
{
    [HarmonyPostfix]
    public static void Postfix(RunState __result)
    {
        // set total players to use in other patches
        MainFile.TotalPlayers = __result.Players.Count;
        
        var totalHealth = __result.Players.Sum(player => player.Creature.MaxHp);

        foreach (var player in __result.Players)
        {
            CreatureCmd.SetMaxAndCurrentHp(player.Creature, totalHealth);
        }
    }
}