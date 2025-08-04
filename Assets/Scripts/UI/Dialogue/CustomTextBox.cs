using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
// 挂载在文本和图片的父对象下，用于自动调整大小
public class CustomTextBox : MonoBehaviour
{
    public float paddingHorizontal = 10;
    public float paddingVertical = 10;

    public float minWidth;   // 最小宽度
    public float maxWidth;   // 最大宽度

    private RectTransform rectTransform;
    private Text text;     // 文本组件

    public void Awake()
    {
        rectTransform = transform as RectTransform;
        text = GetComponentInChildren<Text>();
        UpdateSize();
    }

    // 根据内容和父物体宽度动态刷新尺寸
    public void UpdateSize()
    {
        var textRectTransform = text.transform as RectTransform;
        //获得当前宽度
        float preferredWidth = text.preferredWidth;
        //限制宽度在最大/父对象和最小之间
        preferredWidth = Mathf.Clamp(preferredWidth + paddingHorizontal * 2, minWidth, maxWidth);
        //设置当前宽度
        rectTransform.sizeDelta = new Vector2(preferredWidth, textRectTransform.sizeDelta.y + paddingVertical * 2);

        textRectTransform.anchoredPosition = new Vector2(paddingHorizontal, -paddingVertical);

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    private void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            UpdateSize();
        }
    }
}
