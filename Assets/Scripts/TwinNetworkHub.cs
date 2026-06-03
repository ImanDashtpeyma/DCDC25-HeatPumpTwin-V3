using Photon.Pun;
using UnityEngine;
using System.Collections;

public class TwinNetworkHub : MonoBehaviourPun
{
    private HVACIndicator _indicator;
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
        else
        {
            // Technician هم indicator رو پیدا کنه
            StartCoroutine(FindIndicatorAfterSpawn());
        }
    }
    IEnumerator FindIndicatorAfterSpawn()
    {
        // صبر کن تا HVAC توسط MasterClient spawn بشه
        float timeout = 5f;
        while (_indicator == null && timeout > 0)
        {
            timeout -= Time.deltaTime;
            var hvac = GameObject.Find("hvac 1(Clone)");
            if (hvac != null)
                _indicator = hvac.GetComponentInChildren<HVACIndicator>();
            yield return null;
        }
        _indicator?.SetPending();
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
        _indicator = _spawnedHVAC.GetComponentInChildren<HVACIndicator>();
        _indicator?.SetPending();

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
        //For Disabling  rols
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
        _indicator?.SetApproved();

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
        _indicator?.SetRejected();
        Debug.Log("REJECTED");
        // فقط MasterClient publish کنه
        if (PhotonNetwork.IsMasterClient)
            MQTTManager.Instance?.PublishRejected();
    }
}