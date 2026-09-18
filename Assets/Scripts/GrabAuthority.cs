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

    void Awake()
    {
        if (pointableElement == null)
            pointableElement = GetComponentInChildren<PointableElement>();
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
        Debug.Log($"🖐 GrabAuthority: pointer event {evt.Type} on {name} (Object null={Object == null})");
        if (evt.Type != PointerEventType.Select) return;

        if (Object == null)
        {
            Debug.LogWarning("⚠️ GrabAuthority: Object (NetworkObject) is null on Select — can't request authority.");
            return;
        }

        Debug.Log($"🖐 GrabAuthority: HasStateAuthority={Object.HasStateAuthority} before request on {name}");
        if (Object.HasStateAuthority) return;

        Object.RequestStateAuthority();
        Debug.Log($"🖐 GrabAuthority: RequestStateAuthority() called on {name}");
    }
}
