# Changelog

## 1.0.2

- Mass scaling is now load-aware. `CartMassOverride` adds twice the summed mass of items currently in the cart's tray, so an empty cart stays at vanilla 4 and a loaded cart scales proportionally with its payload. Replaces the old flat 6x multiplier, which over-weighted empty carts and under-scaled very heavy loads.
- Dropped the persistent `rb.mass` change at `PhysGrabCart.Start`. That was leaking the mass bonus into weak-grab lifts, door collisions while coasting, and every other cart interaction where no one was actively pushing. Scope is now strictly "during an active push."
- In-cart adhesion now only kicks in below vanilla's 1 m/s threshold (vanilla handles everything above), and skips items moving fast relative to the cart. Fixes thrown valuables snapping in mid-air over the cart and dropping straight down without their throw momentum.

## 1.0.1

- Docs: added before/after comparison GIFs to README (control, back-and-forth, grab-and-spin, basic maneuver). No functional changes.

## 1.0.0

- HeavyMass: scales cart Rigidbody mass and `CartMassOverride` calls. Loaded turn rate returns to roughly empty-cart feel.
- LerpBoost: items sync to cart velocity every physics tick, including angular contribution. No more drift into cart walls on turns.
