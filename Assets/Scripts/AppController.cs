using System;
using System.Collections;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;

// Phase 1 of the PUN2 -> Photon Fusion migration. Replaces NetworkManager.cs.
// Requires the Photon Fusion 2 SDK to be imported and a Fusion App ID configured
// (Assets/Photon/Fusion/Resources/PhotonAppSettings.asset) before this will compile.
// Shared Mode is used so the Engineer/Technician role split mirrors PUN2's
// MasterClient, and so either user can request state authority when grabbing
// a shared object (Phase 3) instead of needing host round-trips.
//
// Session start is now owned by the Meta "Colocation" Building Block chain
// ([BuildingBlock] Network Manager / Custom Matchmaking / Local Matchmaking),
// which finds nearby colocated players over Bluetooth/WiFi and calls
// Runner.StartGame itself. This script no longer starts its own NetworkRunner —
// it just finds the one those blocks already created, listens for role/session
// callbacks on it, and spawns the shared Map once colocation is actually ready.
public class AppController : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkRunner Runner { get; private set; }
    public static bool IsEngineer => Runner != null && Runner.IsRunning && Runner.IsSharedModeMasterClient;

    public static event Action<bool> OnRoleResolved;

    [Header("Networked Prefabs")]
    [Tooltip("Assign the 'Map' prefab here after it has a NetworkObject component and is registered in Fusion's Network Project Config.")]
    [SerializeField] private NetworkObject mapPrefab;

    private bool _roleResolved;

    void Awake()
    {
        StartCoroutine(WaitForRunnerAndHook());
    }

    // [BuildingBlock] Network Manager only holds a NetworkRunner *template* in
    // the scene — Meta.XR.MultiplayerBlocks.Fusion.CustomMatchmakingFusion
    // actually calls Instantiate(_runnerPrefab) at runtime and starts THAT
    // clone, not the scene object. FindObjectOfType<NetworkRunner>() in Awake
    // was grabbing the inert template, so Runner.IsRunning was always false
    // and OnPlayerJoined never reached us.
    //
    // On-device logs also showed CustomMatchmaking's CreateRoom retrying
    // StartGame multiple times before it actually succeeds (3 separate
    // StartGame call stacks before "[Fusion] adding player" appeared) — each
    // retry instantiates a fresh NetworkRunner, so locking onto the FIRST
    // entry in NetworkRunner.Instances risks hooking an abandoned attempt
    // that never connects, while the one that actually succeeds is a later,
    // different instance we'd never look at again.
    //
    // So don't lock onto one instance once and trust it: every frame, rescan
    // all of NetworkRunner.Instances for whichever one is actually running
    // with a real local player, (re)hook callbacks on it if it changed, and
    // resolve the role from whichever of (event callback, this poll) notices
    // first.
    IEnumerator WaitForRunnerAndHook()
    {
        Debug.Log("📡 Waiting for the Colocation building blocks to start the Fusion session...");

        while (!_roleResolved)
        {
            var instances = NetworkRunner.Instances;
            for (int i = instances.Count - 1; i >= 0; i--)
            {
                var candidate = instances[i];
                if (candidate == null || !candidate.IsRunning || !candidate.LocalPlayer.IsRealPlayer)
                    continue;

                if (Runner != candidate)
                {
                    Runner = candidate;
                    Runner.AddCallbacks(this);
                    Debug.Log("✅ Hooked into active NetworkRunner: " + Runner.name);
                }

                ResolveRole(Runner.IsSharedModeMasterClient, "poll");
                break;
            }
            yield return null;
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player != runner.LocalPlayer) return;
        ResolveRole(runner.IsSharedModeMasterClient, "callback");
    }

    void ResolveRole(bool isEngineer, string source)
    {
        if (_roleResolved) return;
        _roleResolved = true;

        Debug.Log($"🚪 Joined room! (via {source}) Engineer={isEngineer}");
        OnRoleResolved?.Invoke(isEngineer);
    }

    // Wired up in the Inspector to [BuildingBlock] Colocation's ColocationController
    // -> "Colocation Ready Callbacks" UnityEvent. Fires locally on each device once
    // that device's own alignment is complete, so this is the correct point to
    // spawn shared content — not OnPlayerJoined, which only means "connected",
    // not "aligned to the same physical spot yet".
    public void OnColocationReady()
    {
        if (!IsEngineer) return; // only the host/Engineer spawns the shared Map

        var cam = Camera.main.transform;
        var forward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        var mapPos = cam.position + forward * 1.5f;
        mapPos.y = 1.5f;

        if (mapPrefab == null)
        {
            Debug.LogError("⚠️ AppController.mapPrefab is not assigned — cannot spawn Map.");
            return;
        }

        Runner.Spawn(mapPrefab, mapPos, Quaternion.Euler(90, 180, 0));
        Debug.Log("✅ Colocation ready — Map spawned at: " + mapPos);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.LogError($"❌ Disconnected: {shutdownReason}");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogError($"⚠️ Connect failed: {reason}");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.LogError($"❌ Disconnected from server: {reason}");
    }

    // Required by INetworkRunnerCallbacks but unused in this project.
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}
