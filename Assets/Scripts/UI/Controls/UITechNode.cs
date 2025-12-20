using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UITechNode : HoverableButton
{
    enum TechNodeState
    {
        Locked,
        ToStudy,
        BeingStudied,
        Complished,
        Queued
    }

    public Text techName;
    public Text progressText;
    public GameObject checkIcon;
    public GameObject lockIcon;
    public RectTransform recipeLayout;
    public GameObject recipeIconPrefab;
    public GameObject baseLayer;
    public RectTransform fillMask;
    public RectTransform queueInfo;
    public Animator gifAnimator;
    public Text orderText;
    public HoverableButton dequeueButton;

    private GameObject fillLayer;
    private ScriptableTechnologyNode techNode;
    private TechNodeState currentState;

    private float originalFillMaskWidth;
    private float originalQueueInfoAnchorPosX;
    private float animTransition = 0.5f;

    protected override void Awake()
    {
        base.Awake();

        originalFillMaskWidth = fillMask.sizeDelta.x;
    }

    public void Init(ScriptableTechnologyNode techNode)
    {
        this.techNode = techNode;

        ObjectBufferPool.Instance.RestoreAllChildren(recipeLayout);
        if (fillLayer != null)
        {
            Destroy(fillLayer);
            fillLayer = null;
        }

        if (queueInfo != null)
        {
            originalQueueInfoAnchorPosX = queueInfo.anchoredPosition.x;
            queueInfo.gameObject.SetActive(false);
            queueInfo.anchoredPosition = Vector2.zero;

            // 点击取消排队按钮，取消当前科技的研究
            dequeueButton.onClick.AddListener(() =>
            {
                TechnologyManager.Instance.StopStudy();
            });
        }

        foreach (var recipe in techNode.recipes)
        {
            var image = ObjectBufferPool.Instance.Get(recipeIconPrefab, recipeLayout).GetComponent<Image>();
            image.sprite = recipe.CardImage;
        }

        techName.text = techNode.techName;

        // 克隆 baseLayer 得到 fillLayer
        fillLayer = Instantiate(baseLayer, fillMask);
        (fillLayer.transform as RectTransform).anchoredPosition = new(1, 0);
        SetColor(fillLayer.transform, ColorManager.Cyan);

        currentState = GetCurrentState();
        Display();
    }

    private void SetColor(Transform layer, Color color)
    {
        foreach (var img in layer.GetComponentsInChildren<Image>(true))
        {
            img.color = color;
        }
    }

    public void RefreshDisplay()
    {
        if (techNode == null) return;

        var newState = GetCurrentState();

        if (currentState == newState && currentState != TechNodeState.BeingStudied) return;

        currentState = newState;
        Display();
    }

    private TechNodeState GetCurrentState()
    {
        if (TechnologyManager.Instance.IsTechNodeComplished(techNode))
            return TechNodeState.Complished;

        if (TechnologyManager.Instance.IsTechNodeBeingStudied(techNode))
            return TechNodeState.BeingStudied;

        if (TechnologyManager.Instance.IsTechNodeLocked(techNode, out _))
            return TechNodeState.Locked;

        return TechNodeState.ToStudy;
    }

    private void Display()
    {
        SetColor(baseLayer.transform, ColorManager.White);

        var progress = TechnologyManager.Instance.GetStudyProgress(techNode);
        //progressText.gameObject.SetActive(true);
        //progressText.text = $"{progress}/{techNode.cost}";
        fillMask.gameObject.SetActive(true);
        fillMask.sizeDelta = new(originalFillMaskWidth * progress / techNode.cost, fillMask.sizeDelta.y);

        lockIcon.SetActive(false);
        checkIcon.SetActive(false);

        switch (currentState)
        {
            case TechNodeState.Locked:
                Locked();
                break;
            case TechNodeState.ToStudy:
                ToStudy();
                break;
            case TechNodeState.BeingStudied:
                BeingStudied();
                break;
            case TechNodeState.Complished:
                Complished();
                break;
            case TechNodeState.Queued:
                Queued();
                break;
        }
    }

    public void Display(ScriptableTechnologyNode techNode)
    {
        Init(techNode);
        Display();
    }

    private void Locked()
    {
        lockIcon.SetActive(true);
        SetColor(baseLayer.transform, ColorManager.DarkGrey);
        fillMask.gameObject.SetActive(false);
        progressText.gameObject.SetActive(false);
        Dequeue();
    }

    private void Complished()
    {
        checkIcon.SetActive(true);
        progressText.gameObject.SetActive(false);
        Dequeue();
    }

    private void BeingStudied()
    {
        Enqueue(true);
    }

    private void ToStudy()
    {
        Dequeue();
    }

    private void Queued()
    {
        Enqueue(false);
    }

    private void Enqueue(bool playGif)
    {
        if (queueInfo == null) return;

        queueInfo.DOKill();
        queueInfo.gameObject.SetActive(true);
        queueInfo.DOAnchorPosX(originalQueueInfoAnchorPosX, animTransition);

        if (playGif)
            gifAnimator.Play("StudyingGif");
        else
            gifAnimator.Play("Default");
    }

    private void Dequeue()
    {
        if (queueInfo == null) return;

        queueInfo.DOKill();
        queueInfo.DOAnchorPosX(0, animTransition).OnComplete(() =>
        {
            queueInfo.gameObject.SetActive(false);
        });
    }
}