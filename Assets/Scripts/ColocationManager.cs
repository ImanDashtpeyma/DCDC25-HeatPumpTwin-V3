using Photon.Pun;
using UnityEngine;
using ExitGames.Client.Photon;

public class ColocationManager : MonoBehaviourPunCallbacks
{
    public static ColocationManager Instance;

    [Header("Scene Root")]
    public Transform sceneRoot;
    public bool isColocated = false;

    [Header("XR Rig")]
    // OVRCameraRig را در Inspector اینجا Assign کنید
    public Transform xrRig;

    void Awake() => Instance = this;

    // ───── Calibration ─────

    // مهندس (MasterClient) این را می‌زند — هر دو روی نقطه مرجع ایستاده‌اند
    public void BroadcastOrigin()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        Vector3 head = Camera.main.transform.position;
        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new Hashtable { { "colocHead", $"{head.x},{head.y},{head.z}" } });
        isColocated = true;
        Debug.Log("📡 Calibration origin set: " + head);
    }

    // تکنسین این را می‌زند — باید روی همان نقطه مرجع ایستاده باشد
    public void AlignToOrigin()
    {
        if (PhotonNetwork.IsMasterClient) return;
        if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("colocHead", out var raw))
        {
            Debug.LogWarning("⚠️ مهندس هنوز کالیبره نکرده.");
            return;
        }

        var parts = raw.ToString().Split(',');
        var engineerHead = new Vector3(
            float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
        var myHead = Camera.main.transform.position;
        var offset = engineerHead - myHead;

        if (xrRig != null)
            xrRig.position += offset;

        isColocated = true;
        Debug.Log($"🎯 Aligned! Offset applied: {offset}");
    }

    // ───── Legacy (used by TwinNetworkHub) ─────

    public void CreateAndShareAnchor(Vector3 worldPosition)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC(nameof(RPC_SyncPosition), RpcTarget.All,
            worldPosition.x, worldPosition.y, worldPosition.z);
        Debug.Log("📡 Position synced: " + worldPosition);
    }

    [PunRPC]
    void RPC_SyncPosition(float x, float y, float z)
    {
        if (sceneRoot != null)
        {
            sceneRoot.position = new Vector3(x, y, z);
            isColocated = true;
            Debug.Log("🎯 Scene repositioned to: " + new Vector3(x, y, z));
        }
    }
}