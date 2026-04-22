# CartFix

Fixes loaded carts in R.E.P.O. feeling heavy, sluggish, or slow to push and turn.

## Why a loaded cart stalls

Empty cart turns at roughly 4 rad/s. Fill it with 70 valuables and it drops to roughly 0.4 rad/s — a 10x stall from the payload alone. A full cart barely corners, feels heavy to push, and takes forever to get up to speed.

`PhysGrabCart.rb.mass` is 8 kg (4 kg while grabbed). 70 valuables at 0.5 kg each is 35 kg sitting inside an 8 kg container. Unity's contact solver gives the payload most of the impulse share any time the cart tries to move, and items physically block the cart from reaching the velocity that `CartSteer` writes each tick.

Whether this is a bug or an intentional weight mechanic isn't clear. The game already halves cart mass on grab (`CartMassOverride(4f)`), suggesting the developers wanted a light, responsive cart. Payload-induced drag just wasn't compensated for.

## How CartFix fixes it

Cart Rigidbody mass is scaled 6x at `PhysGrabCart.Start`, with a matching prefix on `CartMassOverride` so the grabbed/idle ratio stays consistent. Contact resolution stops draining the cart's momentum into the payload, and a loaded cart turns close to the empty-cart rate.

In-cart items sync to cart velocity every physics tick — including the cart's angular velocity at each item's position — so items follow through turns instead of sliding into cart walls or lagging behind.

Host-side only. Install on the host and every player in the lobby benefits. Zero config.

## Compatibility

Pairs with [CartSpeedSync](https://thunderstore.io/c/repo/p/discjenny/CartSpeedSync/): that mod raises the scripted top speed, CartFix makes the cart reach it under load.

Patches `PhysGrabCart.Start`, `PhysGrabCart.CartMassOverride`, `PhysGrabObjectImpactDetector.FixedUpdate`. Should coexist with any mod that doesn't touch those.
