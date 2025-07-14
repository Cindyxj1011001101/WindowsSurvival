using UnityEngine;
using UnityEngine.UI;

public class PlayerBag : BagBase
{
    //private float maxLoad;

    //private float currentLoad;

    //public float MaxLoad => maxLoad;
    //public float CurrentLoad => currentLoad;
    [SerializeField] private Button organizeButton; // 整理背包按钮

    private void Awake()
    {
        EventManager.Instance.AddListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, OnCardsChanged);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, OnCardsChanged);
    }

    private void OnEnable()
    {
        organizeButton.onClick.AddListener(CompactCards);
    }

    private void OnDisable()
    {
        organizeButton.onClick.RemoveListener(CompactCards);
    }

    public void OnCardsChanged(ChangePlayerBagCardsArgs args)
    {
        //AddLoad(args.card.CardData.weight * args.add);
        StateManager.Instance.AddLoad(args.card.CardData.weight * args.add);
    }

    protected override void Init()
    {
        InitBag(GameDataManager.Instance.PlayerBagData);
    }

    //protected override void InitBag(BagRuntimeData runtimeData)
    //{
    //    base.InitBag(runtimeData);

    //    // 计算载重
    //    currentLoad = 0;
    //    foreach (var slot in slots)
    //    {
    //        if (!slot.IsEmpty)
    //            // 因为同样的卡牌重量都是一样的，所以可以这样算
    //            AddLoad(slot.PeekCard().CardData.weight * slot.StackCount);
    //    }
    //    maxLoad = (runtimeData as PlayerBagRuntimeData).maxLoad;

    //    TriggerChangeLoadEvent();
    //}

    public override bool CanAddCard(CardInstance card)
    {
        // 因为背包和装备共用载重
        // 不是从装备中添加的，要看载重够不够
        if ((card.Slot == null || card.Slot.Bag is not EquipmentBag) &&
            StateManager.Instance.curLoad + card.CardData.weight > StateManager.Instance.maxLoad)
            return false;

        // 载重足够则按照父类的判断标准进行判断
        return base.CanAddCard(card);
    }

    //ChangeLoadArgs args = new ChangeLoadArgs();
    //private void AddLoad(float weight)
    //{
    //    currentLoad += weight;
    //    TriggerChangeLoadEvent();
    //}

    //private void TriggerChangeLoadEvent()
    //{
    //    // 触发载重变化的事件
    //    args.currentLoad = currentLoad;
    //    args.maxLoad = maxLoad;
    //    EventManager.Instance.TriggerEvent(EventType.ChangeLoad, args);
    //}
}