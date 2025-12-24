using UnityEngine.UI;

public class UIStudyProgress : UIStateSlider
{
    public Text valueTextReversedColor;

    protected override void DisplayValueText(float curValue, float maxValue)
    {
        base.DisplayValueText(curValue, maxValue);

        valueTextReversedColor.text = valueText.text;
    }
}