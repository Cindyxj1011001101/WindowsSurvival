using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

//[RequireComponent(typeof(UnityEngine.UI.Graphic))]
public class DoubleClickHandler : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("双击时间间隔（秒）")]
    public float doubleClickInterval = 0.3f;

    [Space]
    public UnityEvent onDoubleClick = new UnityEvent();

    private float lastClickTime = 0f;

    public void OnPointerClick(PointerEventData eventData)
    {
        // 检查是否是左键点击
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        // 计算与上次点击的时间间隔
        float timeSinceLastClick = Time.unscaledTime - lastClickTime;

        // 更新上次点击时间
        lastClickTime = Time.unscaledTime;

        // 如果时间间隔小于设定的双击间隔，则触发双击事件
        if (timeSinceLastClick <= doubleClickInterval)
        {
            onDoubleClick.Invoke();
        }
    }
}