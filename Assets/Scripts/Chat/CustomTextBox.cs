using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using DG.Tweening;

// 允许在编辑器模式下执行脚本
[ExecuteAlways]
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
    private TMP_Text text;     // 文本组件
    private RectTransform ImageRect;    // 图片物体的RectTransform
    private RectTransform parentRect;    // 父物体的RectTransform

    private float lastParentWidth = -1f; // 上一次父物体宽度
    private float lastWidth = -1f;       // 上一次设置的宽度

    public System.Action OnSizeChanged;  // 尺寸变化时的回调


    public void Awake()
    {
        if(parentRect == null && transform.parent != null) parentRect = transform.parent.GetComponent<RectTransform>();
        //获取当前对象的RectTransform和TMP_Text
        TextRect =this.transform.GetComponent<RectTransform>();
        text = this.transform.GetComponent<TMP_Text>();
        RefreshSizeIfNeeded();
    }

    // 根据内容和父物体宽度动态刷新尺寸
    public void RefreshSizeIfNeeded()
    {
        //获得当前宽度
        float preferredWidth=text.preferredWidth;
        //限制宽度在最大/父对象和最小之间
        preferredWidth=Mathf.Clamp(preferredWidth,minWidth,maxWidth);
        //设置当前宽高
        TextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
    }

}
