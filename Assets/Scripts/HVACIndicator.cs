using UnityEngine;

public class HVACIndicator : MonoBehaviour
{
    public Renderer indicatorRenderer;

    private static readonly Color ColorGreen = new Color(0f, 1f, 0f);
    private static readonly Color ColorRed = new Color(1f, 0f, 0f);
    private static readonly Color ColorBlue = new Color(0f, 0.5f, 1f);

    public void SetPending() => SetColor(ColorRed);
    public void SetApproved() => SetColor(ColorGreen);
    public void SetRejected() => SetColor(ColorBlue);

    void SetColor(Color c)
    {
        if (indicatorRenderer != null)
            indicatorRenderer.material.color = c;
    }
}