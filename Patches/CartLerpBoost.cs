using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace CartFix.Patches;

// Fills a low-speed gap in vanilla's in-cart adhesion.
//
// PhysGrabObjectImpactDetector.FixedUpdate (v0.3.2 lines 312-323) already
// lerps in-cart items toward cart velocity, but only while the cart is
// moving faster than 1 m/s. Below that, items drift into cart walls during
// slow turns or when the cart is accelerating from rest. This patch handles
// that range.
//
// Two scope guards beyond vanilla's own checks:
//   * Skip above vanilla's 1 m/s threshold, so the two lerps never stack.
//   * Skip when the item is moving fast relative to the cart (thrown in,
//     bouncing off a wall). Without this, thrown valuables got caught
//     mid-air over the cart and dropped straight down with no horizontal
//     momentum.
//
// Host-only in multiplayer: vanilla's FixedUpdate returns early on non-master
// clients, but Harmony Postfix runs regardless, so we repeat the guard.
[HarmonyPatch(typeof(PhysGrabObjectImpactDetector), "FixedUpdate")]
static class CartLerpBoostPatch
{
    const float LerpCoefficient = 15f;        // ~0.3 lerp per tick at 50 Hz
    const float SettledRelativeSpeed = 1.5f;  // m/s; above this the item is still in flight

    static void Postfix(PhysGrabObjectImpactDetector __instance)
    {
        if (!Plugin.Enabled) return;
        if (!__instance.inCart) return;
        if (__instance.isEnemy) return;
        if (__instance.physGrabObject.playerGrabbing.Count != 0) return;
        if (__instance.currentCart == null) return;
        if (__instance.rb == null || __instance.rb.isKinematic) return;
        if (__instance.GetComponent<PlayerTumble>() != null) return;
        if (GameManager.instance.gameMode == 1 && !PhotonNetwork.IsMasterClient) return;

        var cart = __instance.currentCart.GetComponent<PhysGrabCart>();
        if (cart == null) return;
        if (cart.actualVelocity.magnitude > 1f) return;

        var rb = __instance.rb;
        Vector3 targetVel = cart.actualVelocity + Vector3.Cross(
            cart.rb.angularVelocity,
            rb.worldCenterOfMass - cart.rb.worldCenterOfMass);

        if ((rb.velocity - targetVel).magnitude > SettledRelativeSpeed) return;

        float keepY = rb.velocity.y;
        Vector3 newVel = Vector3.Lerp(rb.velocity, targetVel, LerpCoefficient * Time.fixedDeltaTime);
        if (newVel.y > keepY) newVel.y = keepY;
        rb.velocity = newVel;
    }
}
