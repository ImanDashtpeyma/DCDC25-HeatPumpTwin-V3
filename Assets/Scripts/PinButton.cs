using UnityEngine;

public class PinButton : MonoBehaviour
{
    [Header("References")]
    public TwinNetworkHub hub;
    public int pinId;

    void Start()
    {
        if (hub == null)
            hub = FindObjectOfType<TwinNetworkHub>();
    }

    public void OnPinSelected()
    {
        if (hub == null) return;

        // Only the Engineer's press actually reaches SpawnHVAC (see
        // TwinNetworkHub.SelectPin_Request), and this runs locally on the
        // presser's own device before the RPC goes out, so Camera.main here
        // is reliably the Engineer holding the Map. Spawn on the floor, half
        // a meter out in front of them, instead of at a fixed room position —
        // requested so the HVAC always appears near whoever is using the map
        // rather than floating at table height.
        Vector3 spawnPos = ComputeFloorSpawnPosition(out Vector3 forwardFlat);

        // Two prior attempts at a straight 180-flip on the root never lined
        // the settings panel up, because the panel isn't aligned with the
        // prefab root's own forward axis at all: hvac 1.prefab's "UI_Button"
        // (the panel's parent) has a baked m_LocalRotation of
        // {x:0, y:-0.7071068, z:0, w:0.7071068} — an extra -90° around Y
        // relative to the root, independent of whatever rotation we spawn
        // with. Compensating with +90° here cancels that offset out, so the
        // panel's actual world-facing direction is just Quaternion.LookRotation(forwardFlat)
        // instead of that rotated 90° off. If this still isn't right,
        // Unity's UI-facing convention assumption below is inverted — flip
        // forwardFlat to -forwardFlat rather than touching the +90 term,
        // which is read directly from the prefab, not guessed.
        Quaternion spawnRot = Quaternion.LookRotation(forwardFlat, Vector3.up) * Quaternion.Euler(0f, 90f, 0f);

        hub.SelectPin_Request(pinId, spawnPos, spawnRot);
        Debug.Log($"📍 Pin {pinId} selected, spawning HVAC at {spawnPos}");
    }

    private Vector3 ComputeFloorSpawnPosition(out Vector3 forwardFlat)
    {
        const float floorY = 0f; // Quest's floor-relative tracking origin
        const float distance = 0.5f;

        Transform holder = Camera.main != null ? Camera.main.transform : transform;

        forwardFlat = holder.forward;
        forwardFlat.y = 0f;
        if (forwardFlat.sqrMagnitude < 0.0001f)
            forwardFlat = Vector3.forward;
        forwardFlat.Normalize();

        Vector3 spawnPos = holder.position + forwardFlat * distance;
        spawnPos.y = floorY;
        return spawnPos;
    }
}