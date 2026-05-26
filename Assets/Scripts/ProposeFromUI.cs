using UnityEngine;
using TMPro;

public class ProposeFromUI : MonoBehaviour
{
    [Header("References")]
    public TwinNetworkHub hub;

    public TMP_InputField powerInput;
    public TMP_InputField pressureInput;
    public TMP_InputField phaseInput;

    public float defaultPower = 1200f;
    public float defaultPressure = 1f;
    public int defaultPhase = 1;

    void Start()
    {
        // اگه assign نشده، از Scene پیدا کن
        if (hub == null)
            hub = FindObjectOfType<TwinNetworkHub>();
    }

    public void Propose()
    {
        if (hub == null)
        {
            hub = FindObjectOfType<TwinNetworkHub>();
            if (hub == null)
            {
                Debug.LogError("ProposeFromUI: hub is not assigned.");
                return;
            }
        }

        float power = ReadFloat(powerInput, defaultPower);
        float pressure = ReadFloat(pressureInput, defaultPressure);
        int phase = ReadInt(phaseInput, defaultPhase);

        hub.Technician_RequestChange(power, pressure, phase);
    }

    float ReadFloat(TMP_InputField input, float fallback)
    {
        if (input == null) return fallback;
        if (float.TryParse(input.text, out var v)) return v;
        return fallback;
    }

    int ReadInt(TMP_InputField input, int fallback)
    {
        if (input == null) return fallback;
        if (int.TryParse(input.text, out var v)) return v;
        return fallback;
    }
}