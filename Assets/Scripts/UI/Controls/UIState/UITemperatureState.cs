using UnityEngine;
using UnityEngine.UI;

public class UITemperatureState : UIStateSlider
{
    public Sprite[] levels;
    public Image icon;
    public Color[] colors;

    public HoverableButton button;

    public override void SetValue(float value, float maxValue)
    {
        slider.value = value / maxValue;
        valueText.text = $"{value - maxValue / 2:0.0}";
        int level = StateManager.Instance.PlayerStateDict[PlayerStateEnum.BodyTemperature].StateLevel;
        icon.sprite = levels[level];

        var color = colors[level];
        if (button != null)
        {
            button.hoveredColor = color;
        }
        stateNameText.color = valueText.color = slider.fillRect.GetComponent<Image>().color = color;
    }
}