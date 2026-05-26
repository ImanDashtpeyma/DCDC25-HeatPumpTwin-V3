using Photon.Pun;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    //public GameObject playerPrefab;

    void Start()
    {
        Debug.Log("📡 ،trying to connecting to photon...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("✅ Successful connection to Photon server.");
        PhotonNetwork.JoinOrCreateRoom("PassthroughRoom", new Photon.Realtime.RoomOptions(), Photon.Realtime.TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("🚪 We entered the room: Passthrough Room");
        //PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
        PhotonNetwork.Instantiate("NetworkAvatar", Vector3.zero, Quaternion.identity);
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        Debug.LogError($"❌ Connection was lost. Reason: {cause}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"⚠️ Room entry failed. Code: {returnCode} | message: {message}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"⚠️ Room creation failed. Code: {returnCode} | message: {message}");
    }
}