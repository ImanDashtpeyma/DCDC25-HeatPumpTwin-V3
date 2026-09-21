using Fusion;
using UnityEngine;
using System.Collections;

// Migrated from Photon PUN 2 to Photon Fusion (Shared Mode).
// Needs a NetworkObject component on the same GameObject (add via the Unity
// Editor — this cannot be added by hand-editing the scene file safely).
public class TwinNetworkHub : NetworkBehaviour
{
    private HVACIndicator _indicator;
    [Header("Scene References")]
    public GameObject heatPumpRoot;
    public Transform[] pinSpawnPoints;

    [Header("Networked Prefabs")]
    [Tooltip("Assign the 'hvac 1' prefab here after it has a NetworkObject component and is registered in Fusion's Network Project Config.")]
    public NetworkObject hvacPrefab;

    [Header("Twin State")]
    public int selectedPinId = -1;
    public bool pending = false;

    public float pendingPower;
    public float pendingPressure;
    public int pendingPhase;

    public float appliedPower;
    public float appliedPressure;
    public int appliedPhase;

    private NetworkObject _spawnedHVAC;

    public void SelectPin_Request(int pinId, Vector3 pinWorldPosition, Quaternion pinWorldRotation)
    {
        if (!AppController.IsEngineer) return;
        RPC_SelectPin(pinId, pinWorldPosition, pinWorldRotation);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_SelectPin(int pinId, Vector3 pinPos, Quaternion pinRot)
    {
        selectedPinId = pinId;

        // Real colocation (Meta's Colocation Building Block) aligns both users'
        // coordinate frames once, at session start — by the time a pin is picked,
        // pinPos already means the same physical spot for both clients. No extra
        // anchor broadcast/wait is needed here anymore (that was the old
        // ColocationManager translation-hack's job).
        if (AppController.IsEngineer)
        {
            if (_spawnedHVAC != null)
            {
                Runner.Despawn(_spawnedHVAC);
                _spawnedHVAC = null;
            }
            SpawnHVAC(pinPos, pinRot);
        }
        else
        {
            StartCoroutine(FindIndicatorAfterSpawn());
        }
    }

    IEnumerator FindIndicatorAfterSpawn()
    {
        // صبر کن تا HVAC توسط Engineer spawn بشه
        float timeout = 5f;
        while (_indicator == null && timeout > 0)
        {
            timeout -= Time.deltaTime;
            var hvac = GameObject.Find("hvac 1(Clone)");
            if (hvac != null)
                _indicator = hvac.GetComponentInChildren<HVACIndicator>();
            yield return null;
        }
        _indicator?.SetOff();
    }

    void SpawnHVAC(Vector3 spawnPos, Quaternion spawnRot)
    {
        if (hvacPrefab == null)
        {
            Debug.LogError("⚠️ TwinNetworkHub.hvacPrefab is not assigned — cannot spawn HVAC.");
            return;
        }

        _spawnedHVAC = Runner.Spawn(hvacPrefab, spawnPos, spawnRot);
        heatPumpRoot = _spawnedHVAC.gameObject;
        _indicator = _spawnedHVAC.GetComponentInChildren<HVACIndicator>();
        _indicator?.SetOff();

        // Selecting a pin doesn't change the real unit's state (color/relay)
        // — nothing's been proposed/approved/rejected yet — but Iman wants
        // the audible cue back, so send a "selected" message the Arduino
        // treats as beep-only.
        if (AppController.IsEngineer)
            MQTTManager.Instance?.PublishSelected();
        Debug.Log("✅ HVAC spawned at: " + spawnPos);
    }

    public void ToggleHVAC()
    {
        if (!AppController.IsEngineer) return;
        if (_spawnedHVAC != null)
        {
            Runner.Despawn(_spawnedHVAC);
            _spawnedHVAC = null;
            heatPumpRoot = null;
        }
    }

    public void Technician_RequestChange(float power, float pressure, int phase)
    {
        // Same shape as the RPC_Approve bug: this blocked Engineer callers,
        // but the "Propose" button (ProposeFromUI) lives inside
        // Engineer_Panel and is the only Propose button reachable in a solo
        // test — the guard made it silently do nothing. Allow either role.
        RPC_RequestChange(power, pressure, phase);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_RequestChange(float power, float pressure, int phase)
    {
        pending = true;
        pendingPower = power;
        pendingPressure = pressure;
        pendingPhase = phase;
        _indicator?.SetSuspended();
        Debug.Log($"PENDING: power={power}, pressure={pressure}, phase={phase}");

        // فقط Engineer publish کنه — همون الگوی Approve/Reject، تا فقط یه
        // کلاینت پیام رو به آردوینوی واقعی بفرسته.
        if (AppController.IsEngineer)
            MQTTManager.Instance?.PublishSuspended();
    }

    public void Engineer_Approve()
    {
        if (!AppController.IsEngineer) return;
        RPC_Approve();
    }

    public void Engineer_Reject()
    {
        if (!AppController.IsEngineer) return;
        RPC_Reject();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_Approve()
    {
        // Used to require `pending` (only set true by a Technician's
        // Technician_RequestChange) before doing anything — meaning Approve
        // was a silent no-op in any solo/Engineer-only test, since nothing
        // ever proposed a change. RPC_Reject has no such guard and always
        // worked, which is exactly the asymmetry Iman hit (Reject always
        // responded, Approve never did). Apply unconditionally instead, same
        // as Reject.
        appliedPower = pendingPower;
        appliedPressure = pendingPressure;
        appliedPhase = pendingPhase;
        pending = false;
        _indicator?.SetApproved();

        Debug.Log($"APPROVED: power={appliedPower}");
        // فقط Engineer publish کنه
        if (AppController.IsEngineer)
            MQTTManager.Instance?.PublishApproved();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_Reject()
    {
        pending = false;
        _indicator?.SetRejected();
        Debug.Log("REJECTED");
        // فقط Engineer publish کنه
        if (AppController.IsEngineer)
            MQTTManager.Instance?.PublishRejected();
    }
}
