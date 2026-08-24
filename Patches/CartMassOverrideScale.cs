using System.Collections.Generic;
using CartFix.Services;
using HarmonyLib;

namespace CartFix.Patches;

// Load-aware scale on PhysGrabCart.CartMassOverride.
//
// Vanilla passes a flat 4 while a Semibot steers (CartSteer) and a flat 8 while a
// small cart sits locked (SmallCartLogic), regardless of what is in the tray. Once
// there's real mass in the tray the physics solver transfers the cart's momentum
// into the payload on every contact, which is what makes a full cart feel sluggish.
//
// We sum the masses of everything in itemsInCart and add (factor * load) to the
// override. At factor 2 the cart ends up at least twice as heavy as whatever it's
// carrying, so contacts resolve in the cart's favor. Empty carts pass through at
// the vanilla value and behave identically to vanilla.
//
// Both callers already gate on IsMasterClientOrSingleplayer, so this Prefix is
// host-only and never fires for a cart a client does not own. And because
// CartSteer writes velocity directly (mass-independent), steering input feel is
// unchanged. Only cart-vs-items contact resolution gets heavier.
//
// Stand-down: if CartMassOverride ever arrives with something other than the flat
// 4 / 8 (semiwork made steering load-aware, or another mod got there first), the
// value passes through untouched and the log says so once.
[HarmonyPatch(typeof(PhysGrabCart), "CartMassOverride")]
static class CartMassOverrideScalePatch
{
    static bool warnedUnknownMass;

    static void Prefix(PhysGrabCart __instance, ref float mass)
    {
        if (!Plugin.Enabled) return;

        if (!CartPhysics.IsVanillaFlatMass(mass))
        {
            if (!warnedUnknownMass)
            {
                warnedUnknownMass = true;
                Plugin.Log.LogWarning($"[CartMass] CartMassOverride got {mass:F2}, not the flat 4 / 8 this build was tuned against. Leaving cart mass alone.");
            }
            return;
        }

        mass = CartPhysics.OverrideMass(mass, PayloadMass(__instance.itemsInCart), Plugin.LoadMassFactor.Value);
    }

    static float PayloadMass(List<PhysGrabObject> items)
    {
        float load = 0f;
        for (int i = 0; i < items.Count; i++)
        {
            var pgo = items[i];
            if (pgo == null || pgo.rb == null) continue;
            // massOriginal is what the game resets to after its own OverrideMass;
            // rb.mass may be mid-override. Fall back before its lazy init has run.
            load += pgo.massOriginal > 0f ? pgo.massOriginal : pgo.rb.mass;
        }
        return load;
    }
}
