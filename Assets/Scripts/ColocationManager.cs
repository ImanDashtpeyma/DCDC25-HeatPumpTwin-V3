using Photon.Pun;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ColocationManager : MonoBehaviourPunCallbacks
{
    public static ColocationManager Instance;

    [Header("Scene Root")]
    public Transform sceneRoot;

    private OVRSpatialAnchor _anchor;
    public bool isColocated = false;

    void Awake() => Instance = this;

    public void CreateAndShareAnchor(Vector3 worldPosition)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        StartCoroutine(CreateAnchorRoutine(worldPosition));
    }

    IEnumerator CreateAnchorRoutine(Vector3 worldPosition)
    {
        Debug.Log("⚓ Creating anchor...");
        var go = new GameObject("SharedAnchor");
        go.transform.position = worldPosition;
        _anchor = go.AddComponent<OVRSpatialAnchor>();

        // صبر کن ساخته بشه
        float t = 5f;
        while (!_anchor.Created && t > 0)
        {
            t -= Time.deltaTime;
            yield return null;
        }
        if (!_anchor.Created) { Debug.LogError("❌ Not created"); yield break; }
        Debug.Log("✅ Created: " + _anchor.Uuid);

        // Save با callback
        bool saveDone = false, saveOk = false;
#pragma warning disable CS0618
        _anchor.Save((a, ok) => { saveOk = ok; saveDone = true; });
#pragma warning restore CS0618
        t = 5f;
        while (!saveDone && t > 0) { t -= Time.deltaTime; yield return null; }
        if (!saveOk) { Debug.LogError("❌ Save failed"); yield break; }
        Debug.Log("☁️ Saved!");

        // UUID رو به Client بفرست
        photonView.RPC(nameof(RPC_LoadAnchor), RpcTarget.Others, _anchor.Uuid.ToString());
        RepositionScene(go.transform);
    }

    [PunRPC]
    void RPC_LoadAnchor(string uuidStr)
    {
        StartCoroutine(LoadAnchorRoutine(new Guid(uuidStr)));
    }

    IEnumerator LoadAnchorRoutine(Guid uuid)
    {
        Debug.Log("📥 Loading: " + uuid);

        var unboundList = new List<OVRSpatialAnchor.UnboundAnchor>();
        bool done = false;

        // SDK 74 async load
        var task = OVRSpatialAnchor.LoadUnboundAnchorsAsync(
            new List<Guid> { uuid }, unboundList);

        while (!task.IsCompleted) yield return null;

        if (unboundList.Count == 0)
        {
            Debug.LogError("❌ Anchor not found in cloud!");
            yield break;
        }

        // Bind به یه GameObject جدید
        var go = new GameObject("SharedAnchor_Client");
        var anchor = go.AddComponent<OVRSpatialAnchor>();
        unboundList[0].BindTo(anchor);

        // صبر کن localized بشه
        float t = 10f;
        while (!anchor.Localized && t > 0)
        {
            t -= Time.deltaTime;
            yield return null;
        }

        if (!anchor.Localized)
        {
            Debug.LogError("❌ Anchor not localized!");
            yield break;
        }

        Debug.Log("✅ Localized!");
        RepositionScene(go.transform);
    }

    void RepositionScene(Transform anchorTransform)
    {
        if (sceneRoot == null) return;
        sceneRoot.SetParent(anchorTransform, true);
        isColocated = true;
        Debug.Log("🎯 COLOCATED!");
    }
}