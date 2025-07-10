using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

//[RequireComponent(typeof(UnityEngine.UI.Graphic))]
public class DoubleClickHandler : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("双击间隔")]
    public float doubleClickInterval = 0.3f;

    [Space]
    public UnityEvent onDoubleClick = new UnityEvent();

    private float lastClickTime = 0f;

    public void OnPointerClick(PointerEventData eventData)
    {
        // 判断是否是左键点击
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        // 计算上次点击的时间
        float timeSinceLastClick = Time.unscaledTime - lastClickTime;

        // 记录当前点击的时间
        lastClickTime = Time.unscaledTime;

        // 如果时间间隔小于设置的点击间隔，则触发双击事件
        if (timeSinceLastClick <= doubleClickInterval)
        {
            onDoubleClick.Invoke();
        }
    }
}