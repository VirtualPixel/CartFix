using CartFix.Services;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace CartFix.Patches;

// Fills a low-speed gap in vanilla's in-cart adhesion.
//
// PhysGrabObjectImpactDetector.FixedUpdate (public-2026-07-05 lines 545-556)
// already lerps in-cart items toward cart velocity, but only while the cart is
// moving faster than 1 m/s. Below that, items drift into cart walls during slow
// turns or when the cart is accelerating from rest. This patch handles that range.
//
// Scope guards beyond vanilla's own checks:
//   * Skip whenever vanilla wrote the item's velocity this tick. The Prefix
//     snapshots it, the Postfix compares. That is what keeps the two lerps from
//     stacking even if semiwork lowers their threshold; the 1 m/s check is just
//     the cheap early out for the case we know about today.
//   * Skip when the item is moving fast relative to the cart (thrown in,
//     bouncing off a wall). Without this, thrown valuables got caught mid-air
//     over the cart and dropped straight down with no horizontal momentum.
//
// Host-only in multiplayer: vanilla's FixedUpdate returns early on non-master
// clients, but a Harmony Postfix runs regardless, so we repeat the guard. A
// client's carts are kinematic anyway (PhysGrabObject.Start), which the rb check
// catches as well.
[HarmonyPatch(typeof(PhysGrabObjectImpactDetector), "FixedUpdate")]
static class CartLerpBoostPatch
{
    static void Prefix(PhysGrabObjectImpactDetector __instance, out Vector3 __state)
    {
        __state = __instance.inCart && __instance.rb != null ? __instance.rb.velocity : Vector3.zero;
    }

    static void Postfix(PhysGrabObjectImpactDetector __instance, Vector3 __state)
    {
        if (!Plugin.Enabled) return;
        if (!__instance.inCart) return;
        if (GameManager.instance.gameMode == 1 && !PhotonNetwork.IsMasterClient) return;
        if (__instance.isEnemy) return;
        if (__instance.physGrabObject.playerGrabbing.Count != 0) return;

        var cart = __instance.currentCart;
        if (cart == null || cart.rb == null) return;
        var rb = __instance.rb;
        if (rb == null || rb.isKinematic) return;
        if (rb.velocity != __state) return;
        if (cart.actualVelocity.magnitude > CartPhysics.VanillaLerpSpeed) return;
        if (__instance.GetComponent<PlayerTumble>() != null) return;

        Vector3 targetVel = CartPhysics.CartVelocityAt(
            cart.actualVelocity,
            cart.rb.angularVelocity,
            rb.worldCenterOfMass - cart.rb.worldCenterOfMass);
        if (CartPhysics.StillInFlight(rb.velocity, targetVel)) return;

        rb.velocity = CartPhysics.PullTowardCart(rb.velocity, targetVel, Time.fixedDeltaTime);
    }
}
