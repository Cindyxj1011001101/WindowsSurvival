using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class CustomMessageLayout : MonoBehaviour
{
    //间距
    public float Spacing;
    private GameObject ScrollView;
    private GameObject InputLine;
    private GameObject MessageSpace;
    public void Awake()
    {
        ScrollView = this.transform.Find("ScrollView").gameObject;
        InputLine = this.transform.Find("InputLine").gameObject;
        MessageSpace = this.transform.Find("MessageSpace").gameObject;
    }
    public void Refresh()
    {
        float height = 0;
        if (MessageSpace.activeSelf)
        {
            if (MessageSpace.transform.childCount != 0)
            {
                height += Spacing;
                foreach (Transform message in MessageSpace.transform)
                {
                    message.GetComponentInChildren<CustomTextBox>().UpdateSize();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(message.GetComponent<RectTransform>());
                    message.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -height);
                    height += message.GetComponent<RectTransform>().rect.height + Spacing;
                }
            }
        }
        MessageSpace.GetComponent<RectTransform>().sizeDelta = new Vector2(MessageSpace.GetComponent<RectTransform>().sizeDelta.x, height);
        LayoutRebuilder.ForceRebuildLayoutImmediate(MessageSpace.GetComponent<RectTransform>());
        InputLine.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, height);
        float ScrollViewheight = height + InputLine.GetComponent<RectTransform>().rect.height;
        ScrollView.GetComponent<RectTransform>().offsetMin = new Vector2(ScrollView.GetComponent<RectTransform>().offsetMin.x, ScrollViewheight);
        LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollView.GetComponent<RectTransform>());
        ScrollView.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 0;


    }
}