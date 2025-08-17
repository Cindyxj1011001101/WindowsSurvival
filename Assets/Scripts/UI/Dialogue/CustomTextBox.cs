using UnityEngine;
using UnityEngine.UI;

// 挂载在文本和图片的父对象下，用于自动调整大小
public class CustomTextBox : MonoBehaviour, IAdaptiveSize
{
    public float textPaddingHorizontal = 10;
    public float textPaddingVertical = 10;

    public float boxPaddingHorizontal = 42;

    public float minWidth = 0;

    public bool alwaysMaxWidth = false;

    private RectTransform rectTransform;
    private Text text;     // 文本组件
    private RectTransform textRectTransform;

    private RectTransform layoutTransform;

    public void Awake()
    {
        rectTransform = transform as RectTransform;
        text = GetComponentInChildren<Text>();
        textRectTransform = text.transform as RectTransform;
    }

    public void SetText(string text)
    {
        this.text.text = text;
        UpdateSize();
    }

    // 根据内容和父物体宽度动态刷新尺寸
    public void UpdateSize()
    {
        if (layoutTransform == null) layoutTransform = (GetComponentInParent<ILayoutGroup>() as MonoBehaviour).transform as RectTransform;
        textRectTransform.sizeDelta = new Vector2(layoutTransform.rect.width - boxPaddingHorizontal - textPaddingHorizontal * 2, textRectTransform.sizeDelta.y);

        LayoutRebuilder.ForceRebuildLayoutImmediate(textRectTransform);

        //获得当前宽度
        float preferredWidth = text.preferredWidth;

        //限制宽度在最大/父对象和最小之间
        if (alwaysMaxWidth)
            preferredWidth = layoutTransform.rect.width - boxPaddingHorizontal;
        else
            preferredWidth = Mathf.Clamp(preferredWidth + textPaddingHorizontal * 2, minWidth, layoutTransform.rect.width - boxPaddingHorizontal);
        
        //设置当前宽度
        rectTransform.sizeDelta = new Vector2(preferredWidth, textRectTransform.sizeDelta.y + textPaddingVertical * 2);

        // 设置文本偏移
        textRectTransform.anchoredPosition = new Vector2(textRectTransform.pivot.x == 0.5 ? 0 : textPaddingHorizontal, -textPaddingVertical);
    }
}
