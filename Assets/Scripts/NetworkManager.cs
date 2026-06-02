using Photon.Pun;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        Debug.Log("📡 Connecting to Photon...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("✅ Connected to Photon!");
        PhotonNetwork.JoinOrCreateRoom("PassthroughRoom",
            new Photon.Realtime.RoomOptions(),
            Photon.Realtime.TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("🚪 Joined room!");

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate("Map",
                new Vector3(0, 1.5f, 2f),
                Quaternion.Euler(90, 180, 0));
        }
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        Debug.LogError($"❌ Disconnected: {cause}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"⚠️ Join failed: {returnCode} | {message}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"⚠️ Create failed: {returnCode} | {message}");
    }
}