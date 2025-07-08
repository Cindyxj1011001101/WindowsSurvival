using UnityEngine;
using UnityEngine.UI;

public class CardSlot : MonoBehaviour
{
    private Image iconImage;
    private Image fillImage; // 用于显示新鲜度等
    private Text propertyText; // 用于显示数量和耐久等
    private Text nameText;
    private Transform cardTransform;

    private CardData currentCard;

    private PriorityQueue<CardInstance> cardInstanceQueue = new();

    public bool IsEmpty => currentCard == null;
    public CardData Card => currentCard;
    public int StackCount => cardInstanceQueue.Count;

    public bool CanDrag
    {
        get
        {
            if (currentCard == null) return false;
            return currentCard is FoodCardData || currentCard is ResourceCardData || currentCard is ToolCardData;
        }
    }

    public void Init()
    {
        iconImage = transform.Find("Card/Icon").GetComponent<Image>();
        fillImage = transform.Find("Card/Fill").GetComponent<Image>();
        propertyText = transform.Find("Card/Property").GetComponent<Text>();
        nameText = transform.Find("Card/Name").GetComponent<Text>();
        cardTransform = transform.Find("Card");

        ClearSlot();

        EventManager.Instance.AddListener(EventType.ChangeCardProperty, OnCardPropertyChanged);
    }

    private void OnCardPropertyChanged()
    {
        fillImage.gameObject.SetActive(currentCard is FoodCardData);
        propertyText.text = "";
        switch (currentCard)
        {
            case FoodCardData cardData:
                // 保质期无限
                if (cardData.MaxFresh == -1)
                    fillImage.gameObject.SetActive(false);
                // 有保质期
                else
                    fillImage.fillAmount = (float)(cardInstanceQueue.Peek() as FoodCardInstance).CurrentFresh / cardData.MaxFresh;

                if (currentCard.maxStackNum > 1)
                    propertyText.text = $"x{StackCount}";

                break;

            case ResourceCardData:
                if (currentCard.maxStackNum > 1)
                    propertyText.text = $"x{StackCount}";
                break;

            case PlaceCardData:
                break;

            case ResourcePointCardData cardData:
                propertyText.text = $"{(cardInstanceQueue.Peek() as ResourcePointCardInstance).CurrentEndurance} / {cardData.maxEndurance}";
                break;

            case ToolCardData cardData:
                propertyText.text = $"{(cardInstanceQueue.Peek() as ToolCardInstance).CurrentEndurance} / {cardData.maxEndurance}";
                break;

            default:
                break;
        }
    }

    public bool ContainsSimilarCard(string cardName) => !IsEmpty && currentCard.cardName == cardName;
    
    /// <summary>
    /// 能否堆叠，在使用该方法前请务必确认要堆叠的卡牌和这个slot放有的卡牌是同类的
    /// </summary>
    /// <returns></returns>
    public bool CanStack() => StackCount < currentCard.maxStackNum;

    public void AddCard(CardInstance card)
    {
        if (IsEmpty)
        {
            cardTransform.gameObject.SetActive(true);
            currentCard = card.CardData;
            iconImage.sprite = currentCard.cardImage;
            nameText.text = currentCard.cardName;
        }

        cardInstanceQueue.Enqueue(card);

        OnCardPropertyChanged();
    }

    public CardInstance RemoveCard()
    {
        var cardToRemove = cardInstanceQueue.Dequeue();

        if (StackCount == 0)
            ClearSlot();
        else
            OnCardPropertyChanged();

        return cardToRemove;
    }

    public CardInstance PeekCard()
    {
        return cardInstanceQueue.Peek();
    }

    public void ClearSlot()
    {
        currentCard = null;
        cardInstanceQueue.Clear();
        iconImage.sprite = null;
        nameText.text = "";
        propertyText.text = "";
        cardTransform.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.ChangeCardProperty, OnCardPropertyChanged);
    }
}