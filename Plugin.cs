using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace CartFix;

[BepInPlugin("Vippy.CartFix", "CartFix", BuildInfo.Version)]
public class Plugin : BaseUnityPlugin
{
    // Cart mass override while being steered: vanilla 4 plus this factor times
    // the summed mass of items in the cart. Config-bound so a game rebalance
    // doesn't need a rebuild to tune around.
    internal static ConfigEntry<float> LoadMassFactor = null!;

    // Debug builds flip this from the F5 toggle in Dev/DevTools.cs. Release
    // builds never touch it.
    internal static bool Enabled { get; set; } = true;
    internal static ManualLogSource Log { get; private set; } = null!;

    void Awake()
    {
        Log = Logger;
        LoadMassFactor = Config.Bind("CartFix", "Load mass factor", 2f, new ConfigDescription(
            "Extra cart mass per unit of payload mass while steering. At 2 the cart is always at least " +
            "twice as heavy as its cargo, enough for momentum to survive contacts with the payload. 0 is vanilla.",
            new AcceptableValueRange<float>(0f, 5f)));
        new Harmony(Info.Metadata.GUID).PatchAll();
#if DEBUG
        gameObject.AddComponent<Dev.DevTools>();
#endif
        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} loaded.");
    }
}
