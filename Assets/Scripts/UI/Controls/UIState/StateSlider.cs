using UnityEngine;
using UnityEngine.UI;

public class StateSlider : MonoBehaviour
{
    public Text stateNameText;
    public Text valueText;
    public Slider slider;

    public bool displayPercentage;

    public void SetStateName(string name)
    {
        stateNameText.text = name;
    }

    public void SetValue(float value, float maxValue)
    {
        slider.value = value / maxValue;
        if (displayPercentage)
            valueText.text = $"{value * 100 / maxValue: 0.0}%";
        else
            valueText.text = $"{(int)value}/{maxValue}";
    }

    public void SetValue(PlayerState state)
    {
        SetValue(state.CurValue, state.MaxValue);
    }

    public void SetValue(EnvironmentState state)
    {
        SetValue(state.CurValue, state.MaxValue);
    }
}