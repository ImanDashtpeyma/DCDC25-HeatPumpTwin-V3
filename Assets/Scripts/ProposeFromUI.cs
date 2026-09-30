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

    // The +/- steppers (ValueStepper) and these input fields are purely local
    // UI state — nothing networked. Without this, a Technician's edits only
    // ever existed on their own headset until Propose stored them in the
    // hub's pending* fields, which no visible UI reads, so the Engineer never
    // actually saw what they were being asked to approve. Called from
    // TwinNetworkHub.RPC_RequestChange (runs on every client) to mirror the
    // proposed values into every client's fields. Written with the current
    // culture's ToString(), matching ReadFloat/ReadInt's current-culture parse.
    public void ShowValues(float power, float pressure, int phase)
    {
        if (powerInput != null) powerInput.text = power.ToString();
        if (pressureInput != null) pressureInput.text = pressure.ToString();
        if (phaseInput != null) phaseInput.text = phase.ToString();
    }

    // Called by ValueStepper after a +/- press (and only then — ShowValues
    // writing the fields doesn't call this, so remote updates can't echo
    // back as new broadcasts) so the other headset sees each tap live,
    // before Propose is ever pressed.
    public void BroadcastLiveValues()
    {
        if (hub == null) hub = FindObjectOfType<TwinNetworkHub>();
        if (hub == null) return;

        hub.LiveEdit(
            ReadFloat(powerInput, defaultPower),
            ReadFloat(pressureInput, defaultPressure),
            ReadInt(phaseInput, defaultPhase));
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