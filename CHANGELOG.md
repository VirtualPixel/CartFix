# Changelog

## 1.0.1

- Docs: added before/after comparison GIFs to README (control, back-and-forth, grab-and-spin, basic maneuver). No functional changes.

## 1.0.0

- HeavyMass: scales cart Rigidbody mass and `CartMassOverride` calls. Loaded turn rate returns to roughly empty-cart feel.
- LerpBoost: items sync to cart velocity every physics tick, including angular contribution. No more drift into cart walls on turns.
