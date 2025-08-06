using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatTip : HoverableButton
{
    public RectTransform background;
    public RectTransform mask;
    public CanvasGroup iconEnter;
    public CanvasGroup iconExit;
    public Text text;

    private Sequence showSeq;
    private Sequence pointerEnterSeq;

    private Vector2 showPosOrigin;
    private Vector2 showPosTarget;
    private Vector2 pointerEnterSizeOrigin;
    private Vector2 pointerEnterSizeTarget;

    protected override void Awake()
    {
        base.Awake();
        showPosOrigin = new Vector2(background.anchoredPosition.x + background.sizeDelta.x, background.anchoredPosition.y);
        showPosTarget = background.anchoredPosition;
        pointerEnterSizeOrigin = background.sizeDelta;
        pointerEnterSizeTarget = new Vector2(-(background.parent as RectTransform).anchoredPosition.x, background.sizeDelta.y);
    }

    protected override void Start()
    {
        base.Start();
        // 播放出现动画
        Show();
    }

    public void SetText(string text)
    {
        string newStr = text;
        if (text.Length > 10)
        {
            newStr = text.Substring(0, 10);
            newStr += "...";
        }
        this.text.text = newStr;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        // 打开聊天窗口
        WindowsManager.Instance.OpenWindow("Chat");
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        pointerEnter = true;
        if (showSeq == null || showSeq.IsActive()) return;

        if (pointerEnterSeq != null && pointerEnterSeq.IsActive())
            pointerEnterSeq.Kill();

        pointerEnterSeq = DOTween.Sequence();
        pointerEnterSeq.Join(background.DOSizeDelta(pointerEnterSizeTarget, .2f))
           .Join(iconEnter.DOFade(1, 0.2f))
           .Join(iconExit.DOFade(0, 0.1f));
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        pointerEnter = false;
        if (showSeq == null || showSeq.IsActive()) return;

        if (pointerEnterSeq != null && pointerEnterSeq.IsActive())
            pointerEnterSeq.Kill();

        pointerEnterSeq = DOTween.Sequence();
        pointerEnterSeq.Join(background.DOSizeDelta(pointerEnterSizeOrigin, .2f))
           .Join(iconEnter.DOFade(0, 0.1f))
           .Join(iconExit.DOFade(1, 0.2f));
    }

    private void Show()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = canvasGroup.interactable = false;

        iconEnter.alpha = 0;

        background.anchoredPosition = showPosOrigin;

        showSeq = DOTween.Sequence();
        showSeq.Join(background.DOAnchorPos(showPosTarget, .4f))
           .Join(canvasGroup.DOFade(1f, .2f))
           .OnComplete(() =>
           {
               canvasGroup.alpha = 1f;
               canvasGroup.blocksRaycasts = canvasGroup.interactable = true;
           });
    }

    public void Hide()
    {
        if (isHiding) return;

        isHiding = true;
        transform.SetParent(transform.parent.parent);
        transform.SetAsFirstSibling();

        showSeq?.Kill();
        pointerEnterSeq?.Kill();

        canvasGroup.blocksRaycasts = canvasGroup.interactable = false;

        showSeq = DOTween.Sequence();
        showSeq.Join(background.DOAnchorPos(showPosOrigin, .4f))
           .Join(canvasGroup.DOFade(0f, .2f))
           .OnComplete(() =>
           {
               canvasGroup.alpha = 0f;
               Destroy(gameObject);
           });
    }

    private bool pointerEnter = false;
    private bool isHiding = false;
    private float timer = 0;
    private float timeThreshold = 4f;
    private void Update()
    {
        // 3s未操作后自动隐藏
        if (!pointerEnter && timer < timeThreshold)
        {
            timer += Time.deltaTime;
            if (timer >= timeThreshold)
            {
                Hide();
            }
        }
    }
}