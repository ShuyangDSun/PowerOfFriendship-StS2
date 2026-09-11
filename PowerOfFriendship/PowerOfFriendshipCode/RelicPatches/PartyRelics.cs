using MegaCrit.Sts2.Core.Models;

namespace PowerOfFriendship.PowerOfFriendshipCode.RelicPatches;

internal static class PartyRelics
{
    private static readonly Dictionary<Type, HashSet<RelicModel>> PartyRelicsDict = new();

    private static bool HasRelic(Type relicType)
    {
        HashSet<RelicModel>? relics = PartyRelicsDict.GetValueOrDefault(relicType);
        
        return relics?.Count > 0;
    }

    internal static T? GetRelic<T>() where T : RelicModel
    {
        if (!HasRelic(typeof(T)))
        {
            return null;
        }
        PowerOfFriendship.Logger.Info("GET RELIC: " + typeof(T));
        LogPartyRelics();
        HashSet<RelicModel>? relics = PartyRelicsDict.GetValueOrDefault(typeof(T));
        
        return (T?) relics?.First();
    }

    internal static void Add(RelicModel relic)
    {
        if (!HasRelic(relic.GetType()))
        {
            PartyRelicsDict.Add(relic.GetType(), []);
        }
        PartyRelicsDict[relic.GetType()].Add(relic);
        
        PowerOfFriendship.Logger.Info("ADDING RELIC :" + relic.GetType());
        LogPartyRelics();
    }

    internal static void Remove(RelicModel relic)
    {
        if (!HasRelic(relic.GetType()))
        {
            return;
        }
        
        HashSet<RelicModel> relics = PartyRelicsDict[relic.GetType()];
        relics.Remove(relic);
        
        if (relics.Count == 0)
        {
            PartyRelicsDict.Remove(relic.GetType());
        }
        PowerOfFriendship.Logger.Info("REMOVING RELIC: " + relic.GetType());
        LogPartyRelics();
    }

    private static void LogPartyRelics()
    {
        PowerOfFriendship.Logger.Info("############################ Logging Party Relics ############################");
        foreach (var relics in PartyRelicsDict)
        {
            PowerOfFriendship.Logger.Info($"{relics.Key}: {relics.Value.Count}");
        }
        PowerOfFriendship.Logger.Info("################################## Finished ###################################");
    }
}