using Godot;

using HarmonyLib;

using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace PowerOfFriendship.PowerOfFriendshipCode.UIPatches;

[HarmonyPatch(typeof(AttackIntent))]
internal static class AttackIntentPatch
{
    [HarmonyPatch(nameof(AttackIntent.GetTexture))]
    [HarmonyPrefix]
    private static bool GetTexture(
        ref Texture2D __result,
        AttackIntent __instance,
        IEnumerable<Creature> targets,
        Creature owner)
    {
        int totalDamage = __instance.GetTotalDamage(targets, owner);
        string str = totalDamage >= 5 * PowerOfFriendship.TotalPlayers
            ? (totalDamage >= 10 * PowerOfFriendship.TotalPlayers
                ? (totalDamage >= 20 * PowerOfFriendship.TotalPlayers
                    ? (totalDamage >= 40 * PowerOfFriendship.TotalPlayers
                        ? "5" : "4") : "3") : "2") : "1";
        __result = PreloadManager.Cache.GetTexture2D(ImageHelper.GetImagePath("atlases/intent_atlas.sprites/attack/intent_attack_" + str + ".tres"));
        return false;
    }

    [HarmonyPatch(nameof(AttackIntent.GetAnimation))]
    [HarmonyPostfix]
    private static void GetAnimation(
        ref string __result,
        AttackIntent __instance,
        IEnumerable<Creature> targets,
        Creature owner)
    {
        int totalDamage = __instance.GetTotalDamage(targets, owner);
        string animation = __result[..^2];
        __result = totalDamage >= 5 * PowerOfFriendship.TotalPlayers
            ? (totalDamage >= 10 * PowerOfFriendship.TotalPlayers
                ? (totalDamage >= 20 * PowerOfFriendship.TotalPlayers
                    ? (totalDamage >= 40 * PowerOfFriendship.TotalPlayers
                        ? animation + "_5" : animation + "_4") : animation + "_3") : animation + "_2") : animation + "_1";
    }
}