# CartFix

Fixes loaded carts in R.E.P.O. feeling heavy, sluggish, or slow to push and turn.

<img src="https://raw.githubusercontent.com/VirtualPixel/CartFix/main/media/control.gif" width="800">

*Empty cart, CartFix toggling. No visible difference — the mod only touches loaded behavior, so normal play isn't affected.*

## Why a loaded cart stalls

Empty cart turns at roughly 4 rad/s. Fill it with 70 valuables and it drops to roughly 0.4 rad/s — a 10x stall from the payload alone. A full cart barely corners, feels heavy to push, and takes forever to get up to speed.

`PhysGrabCart.rb.mass` is 8 kg (4 kg while grabbed). 70 valuables at 0.5 kg each is 35 kg sitting inside an 8 kg container. Unity's contact solver gives the payload most of the impulse share any time the cart tries to move, and items physically block the cart from reaching the velocity that `CartSteer` writes each tick.

Whether this is a bug or an intentional weight mechanic isn't clear. The game already halves cart mass on grab (`CartMassOverride(4f)`), suggesting the developers wanted a light, responsive cart. Payload-induced drag just wasn't compensated for.

## Before and after

Each clip is one take of one fully loaded cart, with CartFix toggling mid-clip. Same cart, same input on both sides of the toggle. Watch what the payload does to the cart — and watch that stop once the fix is on.

### Back and forth

<img src="https://raw.githubusercontent.com/VirtualPixel/CartFix/main/media/slow_back_and_forth.gif" width="800">

Slow push, stop, reverse. In vanilla, each direction change nearly parks the cart — the payload drags the momentum out of every reversal. With CartFix, the cart keeps the momentum the input is asking for.

<img src="https://raw.githubusercontent.com/VirtualPixel/CartFix/main/media/fast_back_and_forth.gif" width="800">

Same pattern, harder push. The gap between "what the input says" and "what the cart does" gets worse under load. With the fix, the cart tracks the input the way an empty cart would.

### Grab and spin

<img src="https://raw.githubusercontent.com/VirtualPixel/CartFix/main/media/slow_grab_and_spin.gif" width="800">

Slow rotation while grabbed. Watch the items in the tray: in vanilla, they slide into the cart walls because the cart rotates around them instead of bringing them along. With CartFix, they rotate with the cart.

<img src="https://raw.githubusercontent.com/VirtualPixel/CartFix/main/media/fast_grab_and_spin.gif" width="800">

Fast rotation. Same behavior, more obvious. Items stop bouncing out on turns.

### Basic maneuver

<img src="https://raw.githubusercontent.com/VirtualPixel/CartFix/main/media/slow_basic_maneuver.gif" width="800">

Regular corridor pushing, loaded, slow. A loaded cart in vanilla feels like dragging an anchor. With CartFix it feels like a cart.

<img src="https://raw.githubusercontent.com/VirtualPixel/CartFix/main/media/fast_basic_maneuver.gif" width="800">

Same at speed. Corners tighten, acceleration comes back, items stay put.

## How CartFix fixes it

Cart Rigidbody mass is scaled 6x at `PhysGrabCart.Start`, with a matching prefix on `CartMassOverride` so the grabbed/idle ratio stays consistent. Contact resolution stops draining the cart's momentum into the payload, and a loaded cart turns close to the empty-cart rate.

In-cart items sync to cart velocity every physics tick — including the cart's angular velocity at each item's position — so items follow through turns instead of sliding into cart walls or lagging behind.

Host-side only. Install on the host and every player in the lobby benefits. Zero config.

## Compatibility

Pairs with [CartSpeedSync](https://thunderstore.io/c/repo/p/discjenny/CartSpeedSync/): that mod raises the scripted top speed, CartFix makes the cart reach it under load.

Patches `PhysGrabCart.Start`, `PhysGrabCart.CartMassOverride`, `PhysGrabObjectImpactDetector.FixedUpdate`. Should coexist with any mod that doesn't touch those.
