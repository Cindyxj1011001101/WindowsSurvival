using UnityEngine;
using UnityEngine.UI;

public class UITemperatureState : UIStateSlider
{
    public Sprite[] levels;

    public Image icon;

    public override void SetValue(float value, float maxValue)
    {
        slider.value = value / maxValue;
        valueText.text = $"{value - maxValue / 2:0.0}";
        icon.sprite = levels[StateManager.Instance.GetTemperatureLevel()];
    }
}