using HarmonyLib;

namespace CartFix.Patches;

// Load-aware scale on PhysGrabCart.CartMassOverride.
//
// Vanilla always passes 4f to CartMassOverride while steering, regardless of
// how much the cart is carrying. Once there's real mass in the tray the
// physics solver transfers the cart's momentum into the payload on every
// contact, which is what makes a full cart feel sluggish.
//
// We sum the masses of items currently in itemsInCart and add (factor * load)
// to the override. At factor 2 the cart ends up at least twice as heavy as
// whatever it's carrying, so contacts resolve in the cart's favor. Empty
// carts pass through at the vanilla 4f and behave identically to vanilla.
//
// Scope stays tight to active steering. CartMassOverride is only called from
// CartSteer and SmallCartLogic, both of which already gate on
// IsMasterClientOrSingleplayer — so this Prefix is inherently host-only and
// never fires on remote clients. And because CartSteer writes velocity
// directly (mass-independent), steering input feel is unchanged. Only
// cart-vs-items contact resolution gets heavier while pushing.
//
// Rolled back into game source this would look like:
//     float load = 0f;
//     foreach (var pgo in itemsInCart) {
//         if (pgo == null || pgo.rb == null) continue;
//         load += pgo.massOriginal > 0f ? pgo.massOriginal : pgo.rb.mass;
//     }
//     physGrabObject.OverrideMass(mass + load * LoadMassFactor, 0.1f);
[HarmonyPatch(typeof(PhysGrabCart), "CartMassOverride")]
static class CartMassOverrideScalePatch
{
    static void Prefix(PhysGrabCart __instance, ref float mass)
    {
        if (!Plugin.Enabled) return;

        float loadMass = 0f;
        var items = __instance.itemsInCart;
        for (int i = 0; i < items.Count; i++)
        {
            var pgo = items[i];
            if (pgo == null || pgo.rb == null) continue;
            // Prefer massOriginal (stable base) over rb.mass. rb.mass may be
            // mid-override from OverrideMass on the item itself; massOriginal
            // is the value the game resets back to. Fall back to rb.mass when
            // massOriginal hasn't had its lazy-init pass yet (PhysGrabObject.cs:303).
            loadMass += pgo.massOriginal > 0f ? pgo.massOriginal : pgo.rb.mass;
        }

        mass += loadMass * Plugin.LoadMassFactor;
    }
}
