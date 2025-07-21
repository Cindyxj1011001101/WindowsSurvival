//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

///// <summary>
///// 桌面快捷方式
///// </summary>
//public class DesktopShortcut : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
//{
//    [SerializeField]
//    private Image appIconImage;
//    [SerializeField]
//    private Text appDisplayText;

//    private bool selected = false;

//    private string appName;
//    public string AppName => appName;

//    private Animator animator;

//    private void Awake()
//    {
//        animator = GetComponent<Animator>();
//        DoubleClickHandler doubleClickHandler = GetComponent<DoubleClickHandler>();
//        if (doubleClickHandler == null )
//            doubleClickHandler = gameObject.AddComponent<DoubleClickHandler>();
//        doubleClickHandler.onDoubleClick.AddListener(HandleDoubleClick);
//    }

//    public void Init(App app)
//    {
//        appName = app.name;
//        appIconImage.sprite = app.icon;
//        appDisplayText.text = app.displayText;
//    }

//    public void OnPointerClick(PointerEventData eventData)
//    {
//        // 选中被点击的快捷方式
//        WindowsManager.Instance.Desktop.SelectAppShortcut(appName);
//    }

//    private void HandleDoubleClick()
//    {
//        // 双击打开窗口
//        WindowsManager.Instance.OpenWindow(appName);
//    }

//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        if (!selected)
//            animator.SetTrigger("Highlight");
//    }

//    public void OnPointerExit(PointerEventData eventData)
//    {
//        if (!selected)
//            animator.SetTrigger("Normal");
//    }

//    // 不要自己调用
//    // 不要自己调用
//    // 不要自己调用
//    public void SetSelected(bool selected)
//    {
//        if (this.selected == selected) return;

//        this.selected = selected;
//        if (selected)
//        {
//            animator.SetTrigger("Select");
//        }
//        else
//        {
//            animator.SetTrigger("Normal");
//        }
//    }
//}