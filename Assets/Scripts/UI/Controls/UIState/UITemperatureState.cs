using UnityEngine;
using UnityEngine.UI;

public class UITemperatureState : UIStateSlider
{
    public Sprite[] levels;

    public override void SetValue(float curValue, float maxValue, bool playAnim)
    {
        UpdateSliderValue(curValue, maxValue, playAnim);

        // 根据不同的温度等级显示不同的图片
        int level = CalcLavel(curValue - maxValue / 2);

        if (level < levels.Length)
            icon.sprite = levels[level];

        var color = ColorManager.TemperatureColors[level];
        if (button != null)
        {
            button.hoveredColor = button.currentColor = color;
        }

        icon.color = arrow.color = stateNameText.color = valueText.color = slider.fillRect.GetComponent<Image>().color = color;
    }

    protected override void DisplayValueText(float curValue, float maxValue)
    {
        valueText.text = $"{curValue - maxValue / 2:0.0}";
    }

    private int CalcLavel(float value)
    {
        if (value <= -75)
        {
            return 0;
        }
        else if (value <= -50)
        {
            return 1;
        }
        else if (value <= 50)
        {
            return 2;
        }
        else if (value <= 75)
        {
            return 3;
        }
        else
        {
            return 4;
        }
    }
}