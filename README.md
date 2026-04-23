# CartFix

Fixes loaded carts in R.E.P.O. feeling heavy, sluggish, and slow to push and turn.

<img src="https://raw.githubusercontent.com/VirtualPixel/CartFix/main/media/control.gif" width="800">

*Control clip: empty cart, CartFix toggling mid-way. No visible difference — the mod doesn't touch empty-cart behavior.*

## Why a loaded cart stalls

An empty cart turns at roughly 4 rad/s. Load it up with 70 valuables and that drops to around 0.4 rad/s. A 10x hit from payload alone. The cart barely corners, feels heavy to push, and takes forever to get back up to speed.

The cause is a mass mismatch. `PhysGrabCart.rb.mass` sits around 8 kg, and the game overrides it down to 4 kg while someone is actively pushing. 70 small valuables at 0.5 kg each is 35 kg sitting inside a 4 kg cart. Every physics tick, Unity's contact solver hands the payload most of the impulse share, and the items physically resist the velocity `CartSteer` is trying to write.

Whether this was a deliberate weight mechanic or an oversight isn't clear. Vanilla already halves cart mass during steering, which suggests the cart is supposed to feel light and responsive. The payload drag just wasn't accounted for.

## Comparisons

Each clip is a single take with CartFix toggling in the middle. Same cart, same input on both sides of the toggle. Two speeds per scenario; the faster the input, the more obvious the difference becomes.

### Back and forth push

| Slow | Fast |
| :---: | :---: |
| <img src="https://raw.githubusercontent.com/VirtualPixel/CartFix/main/media/slow_back_and_forth.gif" width="420"> | <img src="https://raw.githubusercontent.com/VirtualPixel/CartFix/main/media/fast_back_and_forth.gif" width="420"> |

Push, stop, reverse. In vanilla every direction change nearly parks the cart because the payload drags momentum out of each reversal. With CartFix the cart carries the momentum the input is asking for. The gap between input and response widens under load, and the harder you push the more obvious it gets.

### Grab and spin

| Slow | Fast |
| :---: | :---: |
| <img src="https://raw.githubusercontent.com/VirtualPixel/CartFix/main/media/slow_grab_and_spin.gif" width="420"> | <img src="https://raw.githubusercontent.com/VirtualPixel/CartFix/main/media/fast_grab_and_spin.gif" width="420"> |

Rotation while grabbed. Watch the items in the tray. In vanilla the cart rotates around them and they end up against the walls. With CartFix they come along with the rotation.

### Basic maneuver

| Slow | Fast |
| :---: | :---: |
| <img src="https://raw.githubusercontent.com/VirtualPixel/CartFix/main/media/slow_basic_maneuver.gif" width="420"> | <img src="https://raw.githubusercontent.com/VirtualPixel/CartFix/main/media/fast_basic_maneuver.gif" width="420"> |

Normal corridor pushing. Corners tighten, acceleration comes back, and items stay where you put them.

## How CartFix fixes it

Two small Harmony patches, each scoped to one specific cart situation.

**Mass during an active push.** When `CartMassOverride` fires during a grab, CartFix sums the masses of items currently in the cart's tray and adds twice that to the override. Empty carts still get the vanilla 4. A loaded cart ends up heavier than its own payload, which is what keeps cart momentum from being drained into items on every contact. Steering input itself is untouched; `CartSteer` writes velocity directly, independent of mass.

**In-cart adhesion below 1 m/s.** Vanilla already lerps in-cart items toward cart velocity once the cart is moving faster than 1 m/s. CartFix adds a softer pull below that threshold so items don't drift into cart walls during slow turns or accel-from-rest. Thrown items keep their momentum. A relative-velocity gate skips items still in flight.

Nothing else is modified. Weak-grab lifts, door collisions while coasting, empty-cart physics: unchanged.

Host-side only. The underlying game methods only run on the host in multiplayer, so mod effects automatically sync to all players via the existing cart replication. Install on the host, everyone in the lobby benefits. No config.

## Compatibility

Pairs well with [CartSpeedSync](https://thunderstore.io/c/repo/p/discjenny/CartSpeedSync/). CartSpeedSync raises the scripted cart speed cap, CartFix makes a loaded cart actually reach it.

Harmony targets: `PhysGrabCart.CartMassOverride` (prefix), `PhysGrabObjectImpactDetector.FixedUpdate` (postfix). Should coexist with any mod that doesn't patch those two methods.
