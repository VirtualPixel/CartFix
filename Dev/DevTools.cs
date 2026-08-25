#if DEBUG
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

namespace CartFix.Dev;

// Debug-build tooling only. F5 toggles mod vs vanilla, F6 bulk-spawns tiny
// valuables for load testing, the HUD shows cart telemetry. Release builds
// compile none of this and Plugin.Enabled stays true.
class DevTools : MonoBehaviour
{
    const KeyCode ToggleKey = KeyCode.F5;
    const KeyCode SpawnKey = KeyCode.F6;
    const int SpawnCount = 100;
    const float SpawnDuration = 5f;
    const float DiagnosticRange = 15f;
    const float CartScanInterval = 0.5f;

    GUIStyle? hudStyle;
    GUIStyle? hudShadow;
    int spawnProgress;
    bool spawning;

    PhysGrabCart? nearestCart;
    float nearestCartDist = float.PositiveInfinity;
    float cartScanTimer;

    void Update()
    {
        if (Input.GetKeyDown(ToggleKey))
        {
            Plugin.Enabled = !Plugin.Enabled;
            Plugin.Log.LogInfo(Plugin.Enabled ? "Enabled" : "Disabled (vanilla)");
        }

        if (Input.GetKeyDown(SpawnKey) && !spawning)
        {
            StartCoroutine(SpawnTinyValuables());
        }

        cartScanTimer -= Time.deltaTime;
        if (cartScanTimer <= 0f)
        {
            cartScanTimer = CartScanInterval;
            RefreshNearestCart();
        }
    }

    void RefreshNearestCart()
    {
        nearestCart = null;
        nearestCartDist = float.PositiveInfinity;

        var player = PlayerController.instance;
        if (player == null) return;

        Vector3 origin = player.transform.position;
        var carts = Object.FindObjectsOfType<PhysGrabCart>();
        for (int i = 0; i < carts.Length; i++)
        {
            var cart = carts[i];
            if (cart == null) continue;
            float d = Vector3.Distance(cart.transform.position, origin);
            if (d < nearestCartDist)
            {
                nearestCartDist = d;
                nearestCart = cart;
            }
        }
    }

    IEnumerator SpawnTinyValuables()
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            Plugin.Log.LogWarning("Only the host can spawn valuables in multiplayer.");
            yield break;
        }

        List<PrefabRef>? tinyRefs = RunManager.instance?.levels?
            .SelectMany(l => l.ValuablePresets.SelectMany(p => p.tiny))
            .Where(r => r != null && r.IsValid())
            .ToList();

        if (tinyRefs == null || tinyRefs.Count == 0)
        {
            Plugin.Log.LogWarning("No tiny valuable prefabs available (not in a run?).");
            yield break;
        }

        var spawnPoint = SemiFunc.LevelPointsGetClosestToLocalPlayer();
        if (spawnPoint == null)
        {
            Plugin.Log.LogWarning("No level point near the player to spawn at.");
            yield break;
        }

        spawning = true;
        spawnProgress = 0;
        try
        {
            Vector3 basePos = spawnPoint.transform.position + Vector3.up;
            Quaternion baseRot = spawnPoint.transform.rotation;
            var delay = new WaitForSeconds(SpawnDuration / SpawnCount);
            bool multiplayer = GameManager.instance.gameMode != 0;

            for (int i = 0; i < SpawnCount; i++)
            {
                var prefab = tinyRefs[Random.Range(0, tinyRefs.Count)];
                Vector3 pos = basePos + new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(0f, 0.6f),
                    Random.Range(-0.5f, 0.5f));

                var go = multiplayer
                    ? PhotonNetwork.InstantiateRoomObject(prefab.ResourcePath, pos, baseRot, 0, null)
                    : Object.Instantiate(prefab.Prefab, pos, baseRot);

                go.GetComponent<ValuableObject>().DollarValueSetLogic();

                spawnProgress = i + 1;
                yield return delay;
            }
        }
        finally
        {
            spawning = false;
        }
    }

    void OnGUI()
    {
        if (hudStyle == null)
        {
            hudStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
            };
            hudShadow = new GUIStyle(hudStyle) { normal = { textColor = Color.black } };
        }

        var shadow = hudShadow!;

        void DrawLabel(float x, float y, string text, Color color)
        {
            GUI.Label(new Rect(x + 2f, y + 2f, 760f, 22f), text, shadow);
            hudStyle!.normal.textColor = color;
            GUI.Label(new Rect(x, y, 760f, 22f), text, hudStyle);
        }

        string status = Plugin.Enabled ? "CartFix: ENABLED" : "CartFix: DISABLED (vanilla)";
        if (spawning) status += $"   Spawning {spawnProgress}/{SpawnCount}";
        Color statusColor = Plugin.Enabled
            ? new Color(0.35f, 0.9f, 0.45f)
            : new Color(0.95f, 0.55f, 0.35f);

        var diag = new List<string>(3);
        if (nearestCart != null && nearestCartDist < DiagnosticRange)
        {
            var rb = nearestCart.rb;
            var pgo = nearestCart.physGrabObject;

            float linVel = nearestCart.actualVelocity.magnitude;
            float angVel = rb != null ? rb.angularVelocity.magnitude : 0f;
            float massNow = rb != null ? rb.mass : 0f;
            float massBase = pgo != null ? pgo.massOriginal : 0f;
            float altMass = pgo != null ? pgo.alterMassValue : 0f;
            bool overrideActive = pgo != null && pgo.timerAlterMass > 0f;

            int itemCount = nearestCart.itemsInCart.Count;
            float loadMass = 0f;
            for (int i = 0; i < nearestCart.itemsInCart.Count; i++)
            {
                var p = nearestCart.itemsInCart[i];
                if (p == null || p.rb == null) continue;
                loadMass += p.massOriginal > 0f ? p.massOriginal : p.rb.mass;
            }
            float addedOverride = loadMass * Plugin.LoadMassFactor.Value;

            diag.Add($"Cart {nearestCartDist:F1}m   lin {linVel:F2} m/s   ang {angVel:F2} rad/s");
            diag.Add(overrideActive
                ? $"mass {massNow:F1}  (base {massBase:F1}, override {altMass:F1})"
                : $"mass {massNow:F1}  (base {massBase:F1}, override -)");
            diag.Add($"items {itemCount} ({loadMass:F1} kg)   +load {addedOverride:F1}");
        }

        const float margin = 14f;
        const float lineH = 20f;
        float startY = Screen.height - margin - (diag.Count + 1) * lineH;

        for (int i = 0; i < diag.Count; i++)
        {
            DrawLabel(margin, startY + i * lineH, diag[i], Color.white);
        }
        DrawLabel(margin, startY + diag.Count * lineH, status, statusColor);
    }
}
#endif
