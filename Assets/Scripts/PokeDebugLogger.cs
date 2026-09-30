using UnityEngine;
using Oculus.Interaction;

// Temporary diagnostic: attach to the same GameObject as a PokeInteractable
// (or one of its parents) to log every raw pointer event it raises. Used to
// tell apart "the poke never touches the collider at all" (no Hover ever
// logged) from "it touches but the gesture/threshold isn't met" (Hover logs,
// Select never does).
public class PokeDebugLogger : MonoBehaviour
{
    private PointableElement _pointable;

    void Awake()
    {
        _pointable = GetComponentInChildren<PointableElement>();
        if (_pointable == null)
            _pointable = GetComponentInParent<PointableElement>();
    }

    void OnEnable()
    {
        if (_pointable != null)
        {
            _pointable.WhenPointerEventRaised += OnEvt;
            Debug.Log("🔎 PokeDebugLogger subscribed on " + name);
        }
        else
        {
            Debug.LogWarning("🔎 PokeDebugLogger: no PointableElement found on/under " + name);
        }
    }

    void OnDisable()
    {
        if (_pointable != null)
            _pointable.WhenPointerEventRaised -= OnEvt;
    }

    void OnEvt(PointerEvent evt)
    {
        Debug.Log($"🔎 PokeDebugLogger: {evt.Type} on {name}");
    }
}
