using UnityEngine;
using UnityEngine.UI;

public class UITimer : UIStateSlider
{
    public VerticalLayoutGroup separatorLayout;

    public override void SetValue(float value, float maxValue)
    {
        base.SetValue(value, maxValue);

        int separatorCount = Mathf.FloorToInt(maxValue / TimeManager.Instance.SettleInterval);

        for (int i = 1; i < separatorLayout.transform.childCount - 1; i++)
        {
            separatorLayout.transform.GetChild(i).gameObject.SetActive(i - 1 < separatorCount);
        }

        separatorLayout.spacing = TimeManager.Instance.SettleInterval * (slider.fillRect.rect.height - (separatorCount - 1) * 2) / maxValue;
    }
}