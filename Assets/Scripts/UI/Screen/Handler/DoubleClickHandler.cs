using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

//[RequireComponent(typeof(UnityEngine.UI.Graphic))]
public class DoubleClickHandler : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("˫��ʱ�������룩")]
    public float doubleClickInterval = 0.3f;

    [Space]
    public UnityEvent onDoubleClick = new UnityEvent();

    private float lastClickTime = 0f;

    public void OnPointerClick(PointerEventData eventData)
    {
        // ����Ƿ���������
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        // �������ϴε����ʱ����
        float timeSinceLastClick = Time.unscaledTime - lastClickTime;

        // �����ϴε��ʱ��
        lastClickTime = Time.unscaledTime;

        // ���ʱ����С���趨��˫��������򴥷�˫���¼�
        if (timeSinceLastClick <= doubleClickInterval)
        {
            onDoubleClick.Invoke();
        }
    }
}