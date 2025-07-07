using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 桌面快捷方式
/// </summary>
public class DesktopShortcut : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Desktop desktop;

    [SerializeField]
    private Image appIconImage;
    [SerializeField]
    private Text appDisplayText;

    private bool selected = false;

    private string appName;
    public string AppName => appName;

    // 双击检测相关变量
    private float doubleClickTimeThreshold = 0.3f; // 双击时间间隔阈值
    private int clickCount = 0;
    private float lastClickTime = 0;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Init(App app, Desktop desktop)
    {
        this.desktop = desktop;
        appName = app.name;
        appIconImage.sprite = app.icon;
        appDisplayText.text = app.displayText;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 选中被点击的快捷方式
        desktop.SelectAppShortcut(appName);

        // 检测双击
        if (Time.time - lastClickTime < doubleClickTimeThreshold)
        {
            clickCount++;

            // 如果是第二次点击且在时间阈值内，则视为双击
            if (clickCount >= 2)
            {
                HandleDoubleClick();
                clickCount = 0;
            }
        }
        else
        {
            clickCount = 1;
        }

        lastClickTime = Time.time;
    }

    private void HandleDoubleClick()
    {
        // 双击打开窗口
        WindowsManager.Instance.OpenWindow(appName);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!selected)
            animator.SetTrigger("Highlight");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!selected)
            animator.SetTrigger("Normal");
    }

    // 不要自己调用
    // 不要自己调用
    // 不要自己调用
    public void SetSelected(bool selected)
    {
        if (this.selected == selected) return;

        this.selected = selected;
        if (selected)
        {
            animator.SetTrigger("Select");
        }
        else
        {
            animator.SetTrigger("Normal");
        }
    }
}