using BaseLib.Config;

using Godot;

using HarmonyLib;

using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Modding;

namespace PowerOfFriendship.PowerOfFriendshipCode;

[ModInitializer(nameof(Initialize))]
public partial class PowerOfFriendship : Node
{
    private const string
        ModId = "PowerOfFriendship";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static int TotalPlayers => Players.Count;
    public static IReadOnlyList<Player> Players { get; internal set; } = [];

    public static void Initialize()
    {
        ModConfigRegistry.Register(ModId, new ModConfig());
        Harmony harmony = new(ModId);

        harmony.PatchAll();
    }
}