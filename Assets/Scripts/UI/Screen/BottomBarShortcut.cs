using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 底边栏快捷方式
/// </summary>
public class BottomBarShortcut : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private BottomBar bottomBar;

    [SerializeField]
    private Image appIconImage;

    private bool selected = false;

    public bool Selected => selected;

    private string appName;
    public string AppName => appName;

    [SerializeField]
    private Animator selectAnimator;
    [SerializeField]
    private Animator appearAnimator;

    private void Start()
    {
        Appear();
    }

    public void Init(App app, BottomBar bottomBar)
    {
        this.bottomBar = bottomBar;
        appName = app.name;
        appIconImage.sprite = app.icon;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (selected)
            WindowsManager.Instance.MinimizeWindow(appName);
        else
            WindowsManager.Instance.FocusWindow(appName);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!selected)
            selectAnimator.SetTrigger("Highlight");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!selected)
            selectAnimator.SetTrigger("Normal");
    }

    // 不要自己调用
    // 不要自己调用
    // 不要自己调用
    public void SetSelected(bool selected)
    {
        if (this.selected == selected) return;

        // 由非选中到选中
        if (selected && !this.selected)
            selectAnimator.SetTrigger("Select");
        // 由选中到非选中
        else
            selectAnimator.SetTrigger("Deselect");

        this.selected = selected;
    }

    // 播放出现的动画
    public void Appear()
    {
        appearAnimator.SetTrigger("Appear");
    }

    // 播放消失的动画
    public void Disappear()
    {
        appearAnimator.SetTrigger("Disappear");
        Destroy(gameObject, .2f);
    }
}