using UnityEngine;

public class FanSpinner : MonoBehaviour
{
    [SerializeField] private float rpm = 60f;
    [SerializeField] private Vector3 spinAxis = Vector3.up;

    public bool IsSpinning { get; set; }

    void Update()
    {
        if (!IsSpinning) return;
        transform.Rotate(spinAxis, rpm / 60f * 360f * Time.deltaTime, Space.Self);
    }
}
