using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// 挂载在文本和图片的父对象下，用于自动调整大小
public class CustomTextBox : MonoBehaviour
{
    [Header("Spacing")]
    public float frontSpace; // 前端空白距离
    public float backSpace;  // 后端空白距离

    [Header("Width Constraints")]
    public float minWidth;   // 最小宽度
    public float maxWidth;   // 最大宽度

    private RectTransform TextRect; // 文本组件的RectTransform
    private Text text;     // 文本组件


    public void Awake()
    {
        //获取当前对象的RectTransform和TMP_Text
        TextRect =this.transform.GetComponent<RectTransform>();
        text = this.transform.GetComponent<Text>();
        RefreshSizeIfNeeded();
    }

    // 根据内容和父物体宽度动态刷新尺寸
    public void RefreshSizeIfNeeded()
    {
        //获得当前宽度
        float preferredWidth=text.preferredWidth;
        //限制宽度在最大/父对象和最小之间
        preferredWidth=Mathf.Clamp(preferredWidth,minWidth,maxWidth);
        //设置当前宽度
        TextRect.GetComponent<RectTransform>().sizeDelta = new Vector2(preferredWidth,TextRect.sizeDelta.y);
        LayoutRebuilder.ForceRebuildLayoutImmediate(TextRect.GetComponent<RectTransform>());
    }

}
