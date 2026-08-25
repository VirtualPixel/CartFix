# Changelog

## 1.0.5 (unreleased)

- Stand-down guards. If `CartMassOverride` arrives with anything but vanilla's flat 4 / 8 (a game-side fix, or another mod's prefix), the value passes through untouched and the log says so once. The in-cart pull now skips any tick where vanilla already moved the item, so the two lerps can't stack even if semiwork lowers their 1 m/s threshold.
- Dropped a redundant `GetComponent<PhysGrabCart>` per in-cart item per physics tick; `currentCart` already is one.
- Cart math lives in `Services/CartPhysics.cs` with an xunit project (`CartFix.Tests`) covering it. Debug-only HUD and spawner moved to `Dev/DevTools.cs`; nothing in the shipped DLL changed there.
- Plugin version comes from the csproj now and the build writes `BuildZip/CartFix.zip` with `manifest.json` synced.
- README: documents the `Load mass factor` config (it said "No config" since 1.0.4), install notes, contact links.

## 1.0.4

- The load mass factor is a config slider now (0 to 5, default 2, 0 is vanilla). The hardcoded constant worked, but a game rebalance would have needed a rebuild to retune around.

## 1.0.3

- **Updated:** Rebuilt for R.E.P.O. v0.4. The patched methods (`PhysGrabCart.CartMassOverride`, `PhysGrabObjectImpactDetector.FixedUpdate`) and every cart field the mod reads are unchanged in v0.4, so a recompile is sufficient and behavior is identical to 1.0.2 on the new game build.

## 1.0.2

- Mass scaling is now load-aware. `CartMassOverride` adds twice the summed mass of items currently in the cart's tray, so an empty cart stays at vanilla 4 and a loaded cart scales proportionally with its payload. Replaces the old flat 6x multiplier, which over-weighted empty carts and under-scaled very heavy loads.
- Dropped the persistent `rb.mass` change at `PhysGrabCart.Start`. That was leaking the mass bonus into weak-grab lifts, door collisions while coasting, and every other cart interaction where no one was actively pushing. Scope is now strictly "during an active push."
- In-cart adhesion now only kicks in below vanilla's 1 m/s threshold (vanilla handles everything above), and skips items moving fast relative to the cart. Fixes thrown valuables snapping in mid-air over the cart and dropping straight down without their throw momentum.

## 1.0.1

- Docs: added before/after comparison GIFs to README (control, back-and-forth, grab-and-spin, basic maneuver). No functional changes.

## 1.0.0

- HeavyMass: scales cart Rigidbody mass and `CartMassOverride` calls. Loaded turn rate returns to roughly empty-cart feel.
- LerpBoost: items sync to cart velocity every physics tick, including angular contribution. No more drift into cart walls on turns.
