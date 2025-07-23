using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class CustomMessageLayout : MonoBehaviour
{


    private GameObject ScrollView;
    private GameObject InputLine;
    private GameObject MessageSpace;
    public void Awake()
    {
        ScrollView = this.transform.Find("Scroll View").gameObject;
        InputLine = this.transform.Find("InputLine").gameObject;
        MessageSpace = this.transform.Find("MessageSpace").gameObject;
    }

    public void Refresh()
    {
        StartCoroutine(RefreshCoroutine());

    }
    private IEnumerator RefreshCoroutine()
    {
        yield return new WaitForSeconds(0.01f);
        foreach (Transform message in MessageSpace.transform)
        {
            message.GetComponentInChildren<CustomTextBox>().RefreshSizeIfNeeded();
            LayoutRebuilder.ForceRebuildLayoutImmediate(message.GetComponent<RectTransform>());
        }
        if (MessageSpace.transform.childCount == 0)
        {
            MessageSpace.GetComponent<VerticalLayoutGroup>().padding.top = 0;
            MessageSpace.GetComponent<VerticalLayoutGroup>().padding.bottom = 0;
        }
        else
        {
            MessageSpace.GetComponent<VerticalLayoutGroup>().padding.top = 10;
            MessageSpace.GetComponent<VerticalLayoutGroup>().padding.bottom = 10;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(MessageSpace.GetComponent<RectTransform>());
        float height = MessageSpace.GetComponent<RectTransform>().rect.height;  

        InputLine.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, height);
        float ScrollViewheight = this.transform.GetComponent<RectTransform>().rect.height - height - InputLine.GetComponent<RectTransform>().rect.height;
        ScrollView.GetComponent<RectTransform>().sizeDelta = new Vector2(ScrollView.GetComponent<RectTransform>().sizeDelta.x, ScrollViewheight);
        StartCoroutine(RefreshScrollView());
    }

    private IEnumerator RefreshScrollView()
    {
        for (int i = 0; i < 2; i++) yield return null;
        ScrollView.GetComponentInChildren<Scrollbar>().value = 0;
    }
}
;