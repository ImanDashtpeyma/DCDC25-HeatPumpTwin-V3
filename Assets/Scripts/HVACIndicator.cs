using UnityEngine;

public class HVACIndicator : MonoBehaviour
{
    public Renderer indicatorRenderer;

    [Tooltip("Fan blades to spin only while Approved (running) and stop otherwise.")]
    [SerializeField] private FanSpinner[] fans;

    private static readonly Color ColorOff = new Color(1f, 0f, 0f);         // red
    private static readonly Color ColorSuspended = new Color(0f, 0.5f, 1f); // blue
    private static readonly Color ColorApproved = new Color(0f, 1f, 0f);    // green
    private static readonly Color ColorRejected = new Color(1f, 0f, 0f);    // red

    // These already run identically on every client (called from
    // TwinNetworkHub's SpawnHVAC/FindIndicatorAfterSpawn/RPC_RequestChange/
    // RPC_Approve/RPC_Reject on both Engineer and Technician), so toggling
    // color/fans here keeps them in sync across peers for free — no extra
    // networked state needed.

    // HVAC just spawned from the Map — off until a Technician proposes a change.
    public void SetOff() { SetColor(ColorOff); SetFansSpinning(false); }

    // Technician pressed Propose — suspended, awaiting the Engineer's review.
    public void SetSuspended() { SetColor(ColorSuspended); SetFansSpinning(false); }

    // Engineer approved — changes applied, unit running.
    public void SetApproved() { SetColor(ColorApproved); SetFansSpinning(true); }

    // Engineer rejected — back to stopped.
    public void SetRejected() { SetColor(ColorRejected); SetFansSpinning(false); }

    void SetColor(Color c)
    {
        if (indicatorRenderer != null)
            indicatorRenderer.material.color = c;
    }

    void SetFansSpinning(bool spinning)
    {
        if (fans == null) return;
        foreach (var fan in fans)
            if (fan != null) fan.IsSpinning = spinning;
    }
}
