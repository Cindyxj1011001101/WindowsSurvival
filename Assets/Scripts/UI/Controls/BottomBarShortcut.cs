using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 底边栏快捷方式
/// </summary>
public class BottomBarShortcut : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image normalImage;
    [SerializeField] private Image hoveredImage;
    [SerializeField] private Color closedColor;

    private bool selected = false;

    public bool Selected => selected;

    public string AppName => name;

    public RectTransform RectTransform => transform as RectTransform;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (selected)
            WindowsManager.Instance.MinimizeWindow(AppName);
        else
            WindowsManager.Instance.OpenWindow(AppName);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoveredImage.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoveredImage.gameObject.SetActive(false);
    }

    // 不要自己调用
    // 不要自己调用
    // 不要自己调用
    public void SetSelected(bool selected)
    {
        if (this.selected == selected) return;

        // 由非选中到选中
        //if (selected && !this.selected)
        //    selectAnimator.SetTrigger("Select");
        //// 由选中到非选中
        //else
        //    selectAnimator.SetTrigger("Deselect");

        this.selected = selected;
    }

    public void SetOpened(bool value)
    {
        //closedCanvasGroup.enabled = !value;
        normalImage.color = value ? Color.white : closedColor;
    }
}