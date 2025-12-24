using UnityEngine;
using UnityEngine.UI;

public class UITimer : UIStateSlider
{
    public VerticalLayoutGroup separatorLayout;

    private float fillRectHeight;
    private float separatorHeight;

    private void Awake()
    {
        fillRectHeight = slider.fillRect.rect.height;
        separatorHeight = (separatorLayout.transform.GetChild(0).transform as RectTransform).rect.height;
    }

    public override void SetValue(float curValue, float maxValue, bool playAnim)
    {
        base.SetValue(curValue, maxValue, playAnim);

        // 显示计时器刻度
        int separatorCount = Mathf.FloorToInt(maxValue / TimeManager.SETTLEMENT_INTERVAL);

        for (int i = 1; i < separatorLayout.transform.childCount - 1; i++)
        {
            separatorLayout.transform.GetChild(i).gameObject.SetActive(i - 1 < separatorCount);
        }

        separatorLayout.spacing = TimeManager.SETTLEMENT_INTERVAL * (fillRectHeight - (separatorCount - 1) * separatorHeight) / maxValue;
    }
}