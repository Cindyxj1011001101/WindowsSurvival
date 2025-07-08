using UnityEngine;
using UnityEngine.UI;

public class CardSlot : MonoBehaviour
{
    private Image icon;
    private Text propertyText;
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

    private void Start()
    {
        icon = transform.Find("Card/Icon").GetComponent<Image>();
        propertyText = transform.Find("Card/Property").GetComponent<Text>();
        nameText = transform.Find("Card/Name").GetComponent<Text>();
        cardTransform = transform.Find("Card");

        ClearSlot();
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
            icon.sprite = currentCard.cardImage;
            nameText.text = currentCard.cardName;
        }

        cardInstanceQueue.Enqueue(card);

        if (currentCard.maxStackNum > 1)
        {
            propertyText.text = $"x{StackCount}";
        }
    }

    public CardInstance RemoveCard()
    {
        var cardToRemove = cardInstanceQueue.Dequeue();

        if (currentCard.maxStackNum > 1)
        {
            propertyText.text = $"x{StackCount}";
        }

        if (StackCount == 0)
        {
            ClearSlot();
        }

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
        icon.sprite = null;
        nameText.text = "";
        propertyText.text = "";
        cardTransform.gameObject.SetActive(false);
    }
}