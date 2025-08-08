using UnityEngine;
using UnityEngine.UI;

public class ChatTipGroup : MonoBehaviour
{
    [SerializeField] int maxChildCount = 3;
    [SerializeField] private GameObject chatTipPrefab;

    public void AddTip(MessageSenderEnum sender, string text)
    {
        Instantiate(chatTipPrefab, transform).GetComponent<ChatTip>().SetText(sender, text);
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        if (transform.childCount > maxChildCount)
        {
            transform.GetChild(0).GetComponent<ChatTip>().Hide();
        }
    }

    public void Clear()
    {
        while (transform.childCount > 0)
        {
            transform.GetChild(0).GetComponent<ChatTip>().Hide();
        }
    }
}