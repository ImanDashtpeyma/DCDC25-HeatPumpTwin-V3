using UnityEngine;
using TMPro;

public class ValueStepper : MonoBehaviour
{
    public TMP_InputField inputField;
    public float stepAmount = 1f;
    public float minValue = 0f;
    public float maxValue = 10000f;

    public void Increase()
    {
        ChangeValue(stepAmount);
    }

    public void Decrease()
    {
        ChangeValue(-stepAmount);
    }

    void ChangeValue(float delta)
    {
        if (inputField == null) return;

        float currentValue = 0f;

        if (!float.TryParse(inputField.text, out currentValue))
            currentValue = 0f;

        currentValue += delta;
        currentValue = Mathf.Clamp(currentValue, minValue, maxValue);

        inputField.text = currentValue.ToString("0");
    }
}