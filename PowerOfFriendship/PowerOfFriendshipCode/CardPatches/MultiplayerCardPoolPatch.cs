using HarmonyLib;

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace PowerOfFriendship.PowerOfFriendshipCode.CardPatches;

[HarmonyPatch(typeof(CardPoolModel), nameof(CardPoolModel.GetUnlockedCards))]
internal static class MultiplayerCardPoolPatch
{
    private static readonly HashSet<Type> MultiplayerCardsToRemove = [
        // Colorless
        typeof(BeaconOfHope),
        typeof(Intercept),
        // Ironclad
        typeof(Tank),
        // Necrobinder
        typeof(LegionOfBone),
    ];
    
    [HarmonyPostfix]
    private static void Postfix(ref IEnumerable<CardModel> __result, CardMultiplayerConstraint multiplayerConstraint)
    {
        if (multiplayerConstraint != CardMultiplayerConstraint.MultiplayerOnly)
        {
            return;
        }
        __result = __result.Where(card => !MultiplayerCardsToRemove.Contains(card.GetType()));
    }
}