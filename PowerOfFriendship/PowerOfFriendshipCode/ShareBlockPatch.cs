// using HarmonyLib;
// using MegaCrit.Sts2.Core.Commands;
// using MegaCrit.Sts2.Core.Entities.Creatures;
// using MegaCrit.Sts2.Core.Localization.DynamicVars;
// using MegaCrit.Sts2.Core.ValueProps;
//
// namespace PowerOfFriendship.PowerOfFriendshipCode;
//
// [HarmonyPatch(typeof(Creature), nameof(Creature.Block), MethodType.Setter)]
// internal static class ShareBlockPatch
// {
//     private static bool synchronizing = false;
//
//     [HarmonyPostfix]
//     private static void BlockPostfix(Creature __instance)
//     {
//         if (synchronizing || !__instance.IsPlayer)
//         {
//             return;
//         }
//
//         synchronizing = true;
//         foreach (Creature ally in __instance.CombatState.Allies)
//         {
//             ally.GainBlockInternal(123);
//             // CreatureCmd.GainBlock(ally, new BlockVar(123, ValueProp.Move));
//         }
//         synchronizing = false;
//     }
// }