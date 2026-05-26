using Photon.Pun;
using UnityEngine;

public class TwinNetworkHub : MonoBehaviourPun
{
    [Header("Scene References")]
    public GameObject heatPumpRoot;
    public Transform[] pinSpawnPoints;

    [Header("Twin State")]
    public int selectedPinId = -1;
    public bool pending = false;

    public float pendingPower;
    public float pendingPressure;
    public int pendingPhase;

    public float appliedPower;
    public float appliedPressure;
    public int appliedPhase;

    private GameObject _spawnedHVAC;

    public void SelectPin_Request(int pinId)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC(nameof(RPC_SelectPin), RpcTarget.All, pinId);
    }

    [PunRPC]
    void RPC_SelectPin(int pinId)
    {
        if (PhotonNetwork.IsMasterClient)
            MQTTManager.Instance?.PublishPending();

        selectedPinId = pinId;

        if (PhotonNetwork.IsMasterClient)
        {
            if (_spawnedHVAC != null)
            {
                PhotonNetwork.Destroy(_spawnedHVAC);
                _spawnedHVAC = null;
            }

            var cam = Camera.main;
            Vector3 spawnPos = cam.transform.position +
                               cam.transform.forward * 1.5f;
            spawnPos.y = 0f;

            _spawnedHVAC = PhotonNetwork.Instantiate(
                "hvac 1", spawnPos, Quaternion.identity);

            if (ColocationManager.Instance != null)
                ColocationManager.Instance.CreateAndShareAnchor(spawnPos);

            heatPumpRoot = _spawnedHVAC;
        }
    }

    public void ToggleHVAC()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (_spawnedHVAC != null)
        {
            PhotonNetwork.Destroy(_spawnedHVAC);
            _spawnedHVAC = null;
            heatPumpRoot = null;
        }
    }

    public void Technician_RequestChange(float power, float pressure, int phase)
    {
        if (PhotonNetwork.IsMasterClient) return;
        photonView.RPC(nameof(RPC_RequestChange), RpcTarget.All, power, pressure, phase);
    }

    [PunRPC]
    void RPC_RequestChange(float power, float pressure, int phase)
    {
        pending = true;
        pendingPower = power;
        pendingPressure = pressure;
        pendingPhase = phase;
        Debug.Log($"PENDING: power={power}, pressure={pressure}, phase={phase}");
    }

    public void Engineer_Approve()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC(nameof(RPC_Approve), RpcTarget.All);
    }

    public void Engineer_Reject()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC(nameof(RPC_Reject), RpcTarget.All);
    }

    [PunRPC]
    void RPC_Approve()
    {
        if (!pending) return;

        appliedPower = pendingPower;
        appliedPressure = pendingPressure;
        appliedPhase = pendingPhase;
        pending = false;

        if (heatPumpRoot != null)
            heatPumpRoot.transform.localScale =
                Vector3.one * (1 + appliedPower / 5000f);

        Debug.Log($"APPROVED: power={appliedPower}");
        MQTTManager.Instance?.PublishApproved();
    }

    [PunRPC]
    void RPC_Reject()
    {
        pending = false;
        Debug.Log("REJECTED");
        MQTTManager.Instance?.PublishRejected();
    }
}