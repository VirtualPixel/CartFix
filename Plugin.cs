using BepInEx;
using HarmonyLib;

namespace CartFix;

[BepInPlugin("Vippy.CartFix", "CartFix", "1.0.1")]
public class Plugin : BaseUnityPlugin
{
    internal const float MassScale = 6f;

    internal Harmony? Harmony { get; set; }

    private void Awake()
    {
        Patch();
        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
    }

    internal void Patch()
    {
        Harmony ??= new Harmony(Info.Metadata.GUID);
        Harmony.PatchAll();
    }
}
