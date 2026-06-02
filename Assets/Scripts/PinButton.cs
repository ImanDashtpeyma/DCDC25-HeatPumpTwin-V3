using UnityEngine;

public class PinButton : MonoBehaviour
{
    [Header("References")]
    public TwinNetworkHub hub;
    public int pinId;

    void Start()
    {
        if (hub == null)
            hub = FindObjectOfType<TwinNetworkHub>();
    }

    public void OnPinSelected()
    {
        if (hub == null) return;
        hub.SelectPin_Request(pinId, transform.position);
        Debug.Log($"📍 Pin {pinId} selected at {transform.position}");
    }
}