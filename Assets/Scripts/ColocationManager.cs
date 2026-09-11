using Fusion;
using UnityEngine;

// Migrated from Photon PUN 2 to Photon Fusion (Shared Mode).
// Still the manual, one-point, translation-only calibration hack (no rotation
// correction) — real native colocation (OVRColocationSession / Shared Spatial
// Anchors) is a separate follow-up phase, not done here. This migration only
// ports the existing behavior onto Fusion so the project compiles again;
// it does not fix the rotation limitation described earlier in this project.
//
// Needs a NetworkObject component on the same GameObject (add via the Unity
// Editor — this cannot be added by hand-editing the scene file safely).
public class ColocationManager : NetworkBehaviour
{
    public static ColocationManager Instance;

    [Header("Scene Root")]
    public Transform sceneRoot;
    public bool isColocated = false;

    [Header("XR Rig")]
    // OVRCameraRig را در Inspector اینجا Assign کنید
    public Transform xrRig;

    private Vector3 _lastEngineerHead;
    private bool _hasEngineerHead;

    void Awake() => Instance = this;

    // ───── Calibration ─────

    // مهندس (Engineer) این را می‌زند — هر دو روی نقطه مرجع ایستاده‌اند
    public void BroadcastOrigin()
    {
        if (!AppController.IsEngineer) return;
        Vector3 head = Camera.main.transform.position;
        RPC_BroadcastOrigin(head);
        isColocated = true;
        Debug.Log("📡 Calibration origin set: " + head);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_BroadcastOrigin(Vector3 engineerHead)
    {
        _lastEngineerHead = engineerHead;
        _hasEngineerHead = true;
    }

    // تکنسین این را می‌زند — باید روی همان نقطه مرجع ایستاده باشد
    public void AlignToOrigin()
    {
        if (AppController.IsEngineer) return;
        if (!_hasEngineerHead)
        {
            Debug.LogWarning("⚠️ مهندس هنوز کالیبره نکرده.");
            return;
        }

        var myHead = Camera.main.transform.position;
        var offset = _lastEngineerHead - myHead;

        if (xrRig != null)
            xrRig.position += offset;

        isColocated = true;
        Debug.Log($"🎯 Aligned! Offset applied: {offset}");
    }

    // ───── Legacy (used by TwinNetworkHub) ─────

    public void CreateAndShareAnchor(Vector3 worldPosition)
    {
        if (!AppController.IsEngineer) return;
        RPC_SyncPosition(worldPosition);
        Debug.Log("📡 Position synced: " + worldPosition);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_SyncPosition(Vector3 pos)
    {
        if (sceneRoot != null)
        {
            sceneRoot.position = pos;
            isColocated = true;
            Debug.Log("🎯 Scene repositioned to: " + pos);
        }
    }
}
