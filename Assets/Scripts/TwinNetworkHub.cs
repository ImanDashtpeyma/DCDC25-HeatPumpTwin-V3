using Photon.Pun;
using UnityEngine;
using System.Collections;

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

    public void SelectPin_Request(int pinId, Vector3 pinWorldPosition)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC(nameof(RPC_SelectPin), RpcTarget.All,
            pinId,
            pinWorldPosition.x,
            pinWorldPosition.y,
            pinWorldPosition.z);
    }

    [PunRPC]
    void RPC_SelectPin(int pinId, float px, float py, float pz)
    {
        selectedPinId = pinId;
        var pinPos = new Vector3(px, py, pz);

        // اول SceneRoot رو sync کن، بعد HVAC spawn کن
        if (ColocationManager.Instance != null)
            ColocationManager.Instance.CreateAndShareAnchor(pinPos);

        if (PhotonNetwork.IsMasterClient)
        {
            if (_spawnedHVAC != null)
            {
                PhotonNetwork.Destroy(_spawnedHVAC);
                _spawnedHVAC = null;
            }

            StartCoroutine(SpawnAfterColocation());
        }
    }

    IEnumerator SpawnAfterColocation()
    {
        // صبر کن تا SceneRoot جابجا بشه
        float timeout = 3f;
        while (ColocationManager.Instance != null &&
               !ColocationManager.Instance.isColocated &&
               timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        // HVAC رو در position صفر نسبت به SceneRoot spawn کن
        var sceneRoot = ColocationManager.Instance?.sceneRoot;
        Vector3 spawnPos = sceneRoot != null ? sceneRoot.position : Vector3.zero;

        _spawnedHVAC = PhotonNetwork.Instantiate("hvac 1", spawnPos, Quaternion.identity);
        heatPumpRoot = _spawnedHVAC;

        MQTTManager.Instance?.PublishPending();
        Debug.Log("✅ HVAC spawned at: " + spawnPos);
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
        // فقط MasterClient publish کنه
        if (PhotonNetwork.IsMasterClient)
            MQTTManager.Instance?.PublishApproved();
    }

    [PunRPC]
    void RPC_Reject()
    {
        pending = false;
        Debug.Log("REJECTED");
        // فقط MasterClient publish کنه
        if (PhotonNetwork.IsMasterClient)
            MQTTManager.Instance?.PublishRejected();
    }
}