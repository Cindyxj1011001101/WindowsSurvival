using UnityEngine;
using UnityEngine.UI;

public class ScrollViewSizeAdapter : MonoBehaviour, IAdaptiveSize
{
    private ILayoutGroup contentLayoutGroup;

    private void Awake()
    {
        contentLayoutGroup = GetComponent<ScrollRect>().content.GetComponent<LayoutGroup>();
    }

    public void UpdateSize()
    {
        if (contentLayoutGroup != null)
            MonoUtility.UpdateLayoutSize(contentLayoutGroup);
    }
}