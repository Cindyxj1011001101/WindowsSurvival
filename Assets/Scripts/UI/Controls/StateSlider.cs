using UnityEngine;
using UnityEngine.UI;

public class StateSlider : MonoBehaviour
{
    public Text valueText;
    public Slider slider;

    public void DisplayState(float value, float maxValue)
    {
        slider.value = value;
        valueText.text = $"{(int)value}/{maxValue}";

    }
}