using UnityEngine;
using UnityEngine.UI;

public class ChatTipGroup : MonoBehaviour
{
    [SerializeField] int maxChildCount = 3;
    [SerializeField] private GameObject chatTipPrefab;
    private Transform[] children;

    public void AddTip(MessageSenderEnum sender, string text, float lifeTime)
    {
        var tip = ObjectBufferPool.Instance.Get(chatTipPrefab, transform).GetComponent<ChatTip>();
        tip.SetText(sender, text);
        tip.SetLifeTime(lifeTime);
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        if (transform.childCount > maxChildCount)
        {
            transform.GetChild(0).GetComponent<ChatTip>().Hide();
        }
    }

    public void Clear()
    {
        children = new Transform[transform.childCount];
        for (int i = 0; i < children.Length; i++)
        {
            children[i] = transform.GetChild(i);
        }
        //transform.DetachChildren(); // 解除所有父子关系

        foreach (Transform child in children)
        {
            child.GetComponent<ChatTip>().Hide();
        }
    }
}