using Photon.Pun;
using UnityEngine;

public class ColocationManager : MonoBehaviourPunCallbacks
{
    public static ColocationManager Instance;

    [Header("Scene Root")]
    public Transform sceneRoot;
    public bool isColocated = false;

    void Awake() => Instance = this;

    public void CreateAndShareAnchor(Vector3 worldPosition)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC(nameof(RPC_SyncPosition),
            RpcTarget.All,
            worldPosition.x,
            worldPosition.y,
            worldPosition.z);

        Debug.Log("📡 Position synced: " + worldPosition);
    }

    [PunRPC]
    void RPC_SyncPosition(float x, float y, float z)
    {
        var pos = new Vector3(x, y, z);

        if (sceneRoot != null)
        {
            sceneRoot.position = pos;
            isColocated = true;
            Debug.Log("🎯 Scene repositioned to: " + pos);
        }
    }
}