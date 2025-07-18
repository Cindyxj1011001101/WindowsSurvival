using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private CardSlot sourceSlot;
    private Canvas canvas;

    private CardSlot cursorSlot;
    private int pickedCount;

    private void Awake()
    {
        sourceSlot = GetComponentInParent<CardSlot>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 在鼠标位置创建图标
        cursorSlot = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/CardSlot"), eventData.position, Quaternion.identity, canvas.transform).GetComponent<CardSlot>();
        cursorSlot.GetComponent<CanvasGroup>().blocksRaycasts = false; // 防止射线命中cursorSlot
        
        if (eventData.button == PointerEventData.InputButton.Left)
            // 左键拖拽
            pickedCount = sourceSlot.StackCount;
        else
            // 右键拖拽
            pickedCount = 1;

        sourceSlot.DisplayCard(sourceSlot.PeekCard(), sourceSlot.StackCount - pickedCount);
        cursorSlot.DisplayCard(sourceSlot.PeekCard(), pickedCount);
    }

    public void OnDrag(PointerEventData eventData)
    {
        (cursorSlot.transform as RectTransform).anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Destroy(cursorSlot.gameObject);
        // 播放放置的音效
        if(SoundManager.Instance != null)
        {SoundManager.Instance.PlaySound("放置卡牌",true);}
        var currentObject = eventData.pointerCurrentRaycast.gameObject;

        BagBase targetBag = currentObject.GetComponentInParent<BagBase>();
        BagBase sourceBag = sourceSlot.GetComponentInParent<BagBase>();

        if (targetBag != null)
        {
            // 同背包放置
            if (targetBag == sourceBag)
            {
                CardSlot targetSlot = currentObject.GetComponentInParent<CardSlot>();
                if (targetSlot != null && targetSlot != sourceSlot)
                {
                    PlaceCardInSameBag(targetSlot, pickedCount);
                }
            }
            // 跨背包放置
            else if (sourceSlot.PeekCard().moveable)
            {
                PlaceCardInDifferentBag(targetBag, pickedCount);
            }
        }

        sourceSlot.RefreshCurrentDisplay();
    }

    private void PlaceCardInSameBag(CardSlot targetSlot, int amount)
    {
        
        for (int i = 0; i < amount; i++)
        {
            if (!targetSlot.CanAddCard(sourceSlot.PeekCard())) break;
            targetSlot.AddCard(sourceSlot.RemoveCard());
        }
    }

    private void PlaceCardInDifferentBag(BagBase targetBag, int amount)
    {
        
        for (int i = 0; i < amount; i++)
        {
            if (!targetBag.CanAddCard(sourceSlot.PeekCard())) break;
            targetBag.AddCard(sourceSlot.RemoveCard());
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && sourceSlot.PeekCard().moveable)
        {
            // 播放放置的音效
            if(SoundManager.Instance != null)
            {SoundManager.Instance.PlaySound("放置卡牌",true);}
            // 右键点击在玩家背包和环境背包之间传送卡牌，一次一张
            BagBase sourceBag = sourceSlot.GetComponentInParent<BagBase>();
            if (sourceBag is PlayerBag && WindowsManager.Instance.IsWindowOpen("EnvironmentBag"))
            {
                PlaceCardInDifferentBag(GameManager.Instance.CurEnvironmentBag, 1);
            }
            else if (sourceBag is EnvironmentBag && WindowsManager.Instance.IsWindowOpen("PlayerBag"))
            {
                PlaceCardInDifferentBag(GameManager.Instance.PlayerBag, 1);
            }
        }
    }
}