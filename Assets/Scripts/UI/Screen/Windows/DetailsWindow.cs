using UnityEngine;
using UnityEngine.UI;

public class DetailsWindow : WindowBase
{
    private Text detailsText;
    private Transform buttonLayout;
    private Transform tagLayout;
    private CardSlot slot;
    private CardSlot sourceSlot;
    private CardInstance currentDisplayedCard;

    protected override void Awake()
    {
        base.Awake();

        slot = transform.Find("Content/CardSlot").GetComponent<CardSlot>();
        detailsText = transform.Find("Content/Details").GetComponent<Text>();
        buttonLayout = transform.Find("Content/ButtonLayout");
        tagLayout = transform.Find("Content/TagLayout");
        // ��ֹ�϶�����
        slot.GetComponentInChildren<CardDragHandler>().enabled = false;
        // ��ֹ˫���¼�
        slot.GetComponentInChildren<DoubleClickHandler>().enabled = false;
    }

    protected override void Init()
    {
    }

    public void SetupSourceSlot(CardSlot sourceSlot)
    {
        // ����ԭ������
        Clear();

        // ��¼sourceSlot�͵�ǰ��ʾ�Ŀ���
        this.sourceSlot = sourceSlot;
        currentDisplayedCard = sourceSlot.PeekCard();

        // ��ʾ����
        slot.AddCard(currentDisplayedCard);

        // ��ʾ���Ʊ�ǩ
        CardData cardData = currentDisplayedCard.GetCardData();
        foreach (var tag in cardData.CardTagList)
        {
            GameObject tagPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/Tags/" + tag.ToString());
            Instantiate(tagPrefab, tagLayout);
        }

        // ��ʾ������ϸ��Ϣ
        detailsText.text = cardData.cardDesc;

        // ��ʾ�ɽ���ѡ��
        foreach (var cardEvent in cardData.cardEventList)
        {
            GameObject buttonPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/Button");
            Button button = Instantiate(buttonPrefab, buttonLayout).GetComponent<Button>();

            // ���cardEvent�Ƿ����㴥������
            if (EffectResolve.Instance.ConditionEventJudge(cardEvent))
            {
                button.GetComponentInChildren<Text>().text = cardEvent.EventName;
                button.onClick.AddListener(() =>
                {
                    EffectResolve.Instance.Resolve(cardEvent);
                });
            }
            else
            {
                button.interactable = false;
            }
        }
    }

    private void Clear()
    {
        slot.ClearSlot();
        sourceSlot = null;
        currentDisplayedCard = null;
        detailsText.text = "";
        for (int i = 0; i < buttonLayout.childCount; i++)
        {
            Destroy(buttonLayout.GetChild(i).gameObject);
        }
        for (int i = 0; i < tagLayout.childCount; i++)
        {
            Destroy(tagLayout.GetChild(i).gameObject);
        }
    }
}
