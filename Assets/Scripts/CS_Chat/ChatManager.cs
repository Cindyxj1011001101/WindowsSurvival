using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ChatManager : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform closedChat;         
    public RectTransform openedChat;         
    public CanvasGroup openedChatGroup;      
    public Image closedChatImage;            

    [Header("动画设置")]
    public float animationDuration = 0.4f;

    [Header("警告闪烁设置")]
    public bool isWarning = false;
    public Color warningColor = Color.red;
    public Color defaultColor = Color.white;
    public float flashInterval = 0.5f;

    private Tween warningTween;

    private Vector2 savedOpenedPos;
    private Vector3 savedOpenedScale;
    private Vector2 closedSize;

    private RectTransform canvasRect;

    private void Start()
    {
        canvasRect = openedChat.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        closedSize = closedChat.sizeDelta;
        savedOpenedPos = openedChat.anchoredPosition;
        savedOpenedScale = openedChat.localScale;
        openedChatGroup.alpha = 0f;
        openedChat.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isWarning)
            StartWarning();
        else
            StopWarning();
    }


    //打开界面
    public void Expand()
    {
        Vector3 worldPos = closedChat.TransformPoint(closedChat.rect.center);
        
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, worldPos, null, out localPoint);
        
        openedChat.gameObject.SetActive(true);
        openedChat.anchoredPosition = localPoint;
        
        Vector2 openedSize = openedChat.sizeDelta;
        float scaleX = closedSize.x / openedSize.x;
        float scaleY = closedSize.y / openedSize.y;
        openedChat.localScale = new Vector3(scaleX, scaleY, 1f);
        openedChatGroup.alpha = 0f;
        
        openedChat.DOAnchorPos(savedOpenedPos, animationDuration);
        openedChat.DOScale(Vector3.one, animationDuration);
        openedChatGroup.DOFade(1f, animationDuration);

        closedChat.GetComponent<Image>().raycastTarget = false;
    }
    //关闭界面
    public void Collapse()
    {
        savedOpenedPos = openedChat.anchoredPosition;
        savedOpenedScale = openedChat.localScale;
        
        Vector3 worldPos = closedChat.TransformPoint(closedChat.rect.center);
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, worldPos, null, out localPoint);
        
        Vector2 openedSize = openedChat.sizeDelta;
        float scaleX = closedSize.x / openedSize.x;
        float scaleY = closedSize.y / openedSize.y;
        
        openedChat.DOAnchorPos(localPoint, animationDuration);
        openedChat.DOScale(new Vector3(scaleX, scaleY, 1f), animationDuration);
        openedChatGroup.DOFade(0f, animationDuration).OnComplete(() =>
        {
            openedChat.gameObject.SetActive(false);
        });
        closedChat.GetComponent<Image>().raycastTarget = true;
    }

    void StartWarning()
    {
        if (warningTween != null || !closedChat.gameObject.activeInHierarchy)
            return;

        warningTween = closedChatImage.DOColor(warningColor, flashInterval)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.Linear);
    }

    void StopWarning()
    {
        if (warningTween != null)
        {
            warningTween.Kill();
            warningTween = null;
            closedChatImage.color = defaultColor;
        }
    }
}
