using Fusion;
using Oculus.Interaction;
using UnityEngine;

// Fixes a real gap in Fusion Shared Mode: only whoever currently holds State
// Authority over a NetworkObject has their local transform changes replicated
// to other peers via NetworkTransform. Map and hvac 1 are spawned by the
// Engineer, who gets State Authority by default — without this script, only
// the Engineer's grabs/moves would sync; a Technician grabbing the same
// object would move it locally with no effect on what the Engineer sees.
// Requesting authority the moment either peer grabs the object fixes both
// directions. Add this component next to the NetworkObject on Map.prefab and
// hvac 1.prefab (same GameObject that already has the Grabbable/PointableElement
// used by GrabInteractable/HandGrabInteractable).
[RequireComponent(typeof(NetworkObject))]
public class GrabAuthority : NetworkBehaviour
{
    [SerializeField] private PointableElement pointableElement;
    private Rigidbody _rigidbody;
    private bool _isGrabbed;
    private float _lastPosLogTime;

    // Bridges Interaction SDK's grab (which moves transform.position every
    // Update(), on the render clock) into Fusion's own simulation clock.
    // On-device logging proved that with PhysicsForecast off (this project's
    // setting), Fusion's NetworkTransform.Render() re-applies the last
    // *networked* position every frame — for every peer, including the state
    // authority owner — because that's all it has: our grab code never wrote
    // into the network state, only into transform.position on a normal
    // Update(), which FixedUpdateNetwork (Fusion's tick, decoupled from
    // render frames) never captured. Result: the object visibly snapped back
    // to its spawn position every single frame, which read on-device as the
    // hand passing straight through it. Caching the grabbed pose here and
    // reapplying it inside FixedUpdateNetwork() gets it into the tick Fusion
    // actually simulates from, so NetworkTransform captures and syncs it.
    private Vector3 _pendingPosition;
    private Quaternion _pendingRotation;
    private bool _hasPendingPose;

    void Awake()
    {
        if (pointableElement == null)
            pointableElement = GetComponentInChildren<PointableElement>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        if (pointableElement != null)
        {
            pointableElement.WhenPointerEventRaised += HandlePointerEvent;
            Debug.Log("🖐 GrabAuthority: subscribed to PointableElement on " + name);
        }
        else
        {
            Debug.LogWarning("⚠️ GrabAuthority: no PointableElement found — grab authority transfer is disabled on " + name);
        }
    }

    void OnDisable()
    {
        if (pointableElement != null)
            pointableElement.WhenPointerEventRaised -= HandlePointerEvent;
    }

    void HandlePointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Select)
        {
            _isGrabbed = true;
            Debug.Log($"🖐 GrabAuthority: pointer event {evt.Type} on {name} (Object null={Object == null})");

            if (Object == null)
            {
                Debug.LogWarning("⚠️ GrabAuthority: Object (NetworkObject) is null on Select — can't request authority.");
                return;
            }

            Debug.Log($"🖐 GrabAuthority: HasStateAuthority={Object.HasStateAuthority} before request on {name}");
            Debug.Log($"📍 GrabAuthority: transform.position={transform.position} rigidbody.position={(_rigidbody != null ? _rigidbody.position.ToString() : "no-rigidbody")} isKinematic={(_rigidbody != null ? _rigidbody.isKinematic.ToString() : "n/a")} on Select");

            if (!Object.HasStateAuthority)
            {
                Object.RequestStateAuthority();
                Debug.Log($"🖐 GrabAuthority: RequestStateAuthority() called on {name}");
            }
        }
        else if (evt.Type == PointerEventType.Unselect)
        {
            _isGrabbed = false;
            _hasPendingPose = false;
            Debug.Log($"📍 GrabAuthority: transform.position={transform.position} on Unselect");
        }
    }

    void Update()
    {
        if (!_isGrabbed) return;

        _pendingPosition = transform.position;
        _pendingRotation = transform.rotation;
        _hasPendingPose = true;

        if (Time.time - _lastPosLogTime < 0.3f) return;
        _lastPosLogTime = Time.time;
        Debug.Log($"📍[Update] GrabAuthority: transform.position={transform.position} rigidbody.position={(_rigidbody != null ? _rigidbody.position.ToString() : "no-rigidbody")} while grabbed on {name}");
    }

    // Fusion's simulation tick. Reapplying the grabbed pose here (instead of
    // only in Update()) is what makes NetworkTransform actually capture and
    // sync it — see the comment on _pendingPosition above.
    public override void FixedUpdateNetwork()
    {
        if (!_isGrabbed || !_hasPendingPose || Object == null || !Object.HasStateAuthority) return;

        transform.position = _pendingPosition;
        transform.rotation = _pendingRotation;
    }

    void LateUpdate()
    {
        if (!_isGrabbed) return;
        if (Time.time - _lastPosLogTime > 0.05f) return;
        Debug.Log($"📍[LateUpdate] GrabAuthority: transform.position={transform.position} rigidbody.position={(_rigidbody != null ? _rigidbody.position.ToString() : "no-rigidbody")} while grabbed on {name}");
    }
}
