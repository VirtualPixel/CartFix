using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace CartFix.Patches;

// Syncs in-cart items to cart velocity every physics tick. Vanilla only does
// this above a cart speed threshold; this runs whenever an item is in the
// cart so items don't drift into the cart walls during acceleration or turns
[HarmonyPatch(typeof(PhysGrabObjectImpactDetector), "FixedUpdate")]
static class CartLerpBoostPatch
{
    const float LerpCoefficient = 50f;

    static void Postfix(PhysGrabObjectImpactDetector __instance)
    {
        if (!__instance.inCart) return;
        if (__instance.isEnemy) return;
        if (__instance.physGrabObject.playerGrabbing.Count != 0) return;
        if (__instance.currentCart == null) return;
        if (__instance.rb == null || __instance.rb.isKinematic) return;
        if (__instance.GetComponent<PlayerTumble>() != null) return;

        if (GameManager.instance != null && GameManager.instance.gameMode == 1
            && !PhotonNetwork.IsMasterClient) return;

        var cart = __instance.currentCart.GetComponent<PhysGrabCart>();
        if (cart == null) return;

        var rb = __instance.rb;
        var cartRb = cart.rb;
        Vector3 cartLinVel = cart.actualVelocity;
        Vector3 cartAngVel = cartRb != null ? cartRb.angularVelocity : Vector3.zero;
        Vector3 cartPos = cartRb != null ? cartRb.worldCenterOfMass : cart.transform.position;
        Vector3 targetVel = cartLinVel + Vector3.Cross(cartAngVel, rb.worldCenterOfMass - cartPos);

        Vector3 itemVel = rb.velocity;
        float keepY = itemVel.y;
        Vector3 newVel = Vector3.Lerp(itemVel, targetVel, LerpCoefficient * Time.fixedDeltaTime);
        if (newVel.y > keepY) newVel.y = keepY;
        rb.velocity = newVel;
    }
}
