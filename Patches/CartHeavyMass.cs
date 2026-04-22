using HarmonyLib;

namespace CartFix.Patches;

// Scales cart Rigidbody mass. CartSteer writes velocity directly so player
// input moves the cart the same regardless of mass. What changes: contact
// resolution with loaded items stops draining the cart's momentum
[HarmonyPatch(typeof(PhysGrabCart), "Start")]
static class CartHeavyStartPatch
{
    static void Postfix(PhysGrabCart __instance)
    {
        var rb = __instance.rb;
        if (rb == null) return;

        rb.mass *= Plugin.MassScale;

        var pgo = __instance.physGrabObject;
        if (pgo != null) pgo.massOriginal *= Plugin.MassScale;
    }
}

// CartMassOverride is called per FixedUpdate during grab with 4f; scale it
// so the grabbed mass keeps the same ratio to idle mass
[HarmonyPatch(typeof(PhysGrabCart), "CartMassOverride")]
static class CartMassOverrideScalePatch
{
    static void Prefix(ref float mass) => mass *= Plugin.MassScale;
}
