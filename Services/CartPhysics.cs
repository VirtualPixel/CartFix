using UnityEngine;

namespace CartFix.Services;

// The numbers behind both patches, kept free of game types so CartFix.Tests can
// run them outside Unity.
static class CartPhysics
{
    // What vanilla passes to PhysGrabCart.CartMassOverride: 4 from CartSteer while a
    // Semibot pushes, 8 from SmallCartLogic while a small cart sits locked.
    public const float SteerMass = 4f;
    public const float LockedSmallCartMass = 8f;

    // Vanilla's own in-cart lerp only runs above this cart speed. We fill in below it.
    public const float VanillaLerpSpeed = 1f;
    const float LerpCoefficient = 15f;         // ~0.3 per tick at 50 Hz
    const float SettledRelativeSpeed = 1.5f;   // m/s; faster than this and the item is still in flight

    public static bool IsVanillaFlatMass(float mass)
    {
        return mass == SteerMass || mass == LockedSmallCartMass;
    }

    // Anything other than the flat 4 / 8 means the game or another mod already
    // changed this value. Stacking our payload term on top would double-fix, so
    // the number passes through untouched.
    public static float OverrideMass(float vanillaMass, float payloadMass, float factor)
    {
        if (!IsVanillaFlatMass(vanillaMass)) return vanillaMass;
        return vanillaMass + payloadMass * factor;
    }

    public static Vector3 CartVelocityAt(Vector3 cartVelocity, Vector3 cartAngularVelocity, Vector3 offsetFromCartCenter)
    {
        return cartVelocity + Vector3.Cross(cartAngularVelocity, offsetFromCartCenter);
    }

    public static bool StillInFlight(Vector3 itemVelocity, Vector3 cartVelocityAtItem)
    {
        return (itemVelocity - cartVelocityAtItem).magnitude > SettledRelativeSpeed;
    }

    // Lerp toward the cart but never add upward velocity, same rule vanilla uses
    // above 1 m/s so items don't hop when the cart starts moving.
    public static Vector3 PullTowardCart(Vector3 itemVelocity, Vector3 cartVelocityAtItem, float fixedDeltaTime)
    {
        Vector3 pulled = Vector3.Lerp(itemVelocity, cartVelocityAtItem, LerpCoefficient * fixedDeltaTime);
        if (pulled.y > itemVelocity.y) pulled.y = itemVelocity.y;
        return pulled;
    }
}
