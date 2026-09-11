using HarmonyLib;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;

using PowerOfFriendship.PowerOfFriendshipCode.RelicPatches;

namespace PowerOfFriendship.PowerOfFriendshipCode;

/// <summary> Sets all player health bars equal (either combined or averaged) and sets TotalPlayers variable </summary>
[HarmonyPatch(typeof(RunState), nameof(RunState.CreateForNewRun))]
internal class CreateNewRunPatch
{
    [HarmonyPostfix]
    private static void Postfix(RunState __result)
    {
        // set total players to use in other patches
        PowerOfFriendship.Players = __result.Players;
        var totalHealth = __result.Players.Sum(player => player.Creature.MaxHp);

        var maxHealth = ModConfig.AverageHealthBar ? totalHealth / __result.Players.Count : totalHealth;
        foreach (var player in __result.Players)
        {
            CreatureCmd.SetMaxHp(player.Creature, maxHealth).GetAwaiter().GetResult();

            foreach (var relic in player.Relics)
            {
                PartyRelics.Add(relic);
            }
        }
    }
}

[HarmonyPatch(typeof(RunState), nameof(RunState.FromSerializable))]
internal class LoadRunPatch
{
    [HarmonyPostfix]
    private static void Postfix(RunState __result)
    {
        PowerOfFriendship.Players = __result.Players;
        foreach (var player in __result.Players)
        {
            foreach (var relic in player.Relics)
            {
                if (!relic.IsMelted || (relic is LizardTail { WasUsed: false }))
                {
                    PartyRelics.Add(relic);
                }
            }
        }
    }
}