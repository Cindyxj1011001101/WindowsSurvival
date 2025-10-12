/// <summary>
/// ËÉ¶¯¾ŞÊ¯
/// </summary>
public class LooseBoulders : Card
{
    private RandomDropList dropList = new(
       new Drop(3, ("²£Á§É³", 1)),
       new Drop(2, ("°×±¬¿ó", 1)),
       new Drop(1, ("º£ÅÀ³æ", 1))
       );

    private LooseBoulders()
    {
        Events = new()
        {
            new CardEvent("ÓÃ²ù×ÓÔä", "", Event_DigByTool, Judge_DigByTool, () => 15),
        };
    }

    public override void Awake()
    {
        base.Awake();

        TryGetComponent<DurabilityComponent>(out var d);
        d.onBroken = () =>
        {
            TurnTo("´ÓÖ¯¹âÔåÄ¹Ô°µ½Ç³²ãÑÒÑ¨", Bag);
        };
    }

    private void Event_DigByTool(out string tip)
    {
        DigByTool(GameManager.Instance.PlayerBag.FindCardOfName("¸Ö²ù"), out tip);
    }

    private bool Judge_DigByTool(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("¸Ö²ù") == null)
        {
            hint = "ĞèÒª¸Ö²ù";
            return false;
        }
        return true;
    }

    private void DigByTool(Card tool, out string tip)
    {
        //µôÂä¿¨ÅÆ
        RandomDrop(dropList, out tip, onDrop: () =>
        {
            //ÏûºÄ1µãÄÍ¾Ã¶È
            Use();
            // ¹¤¾ßÏûºÄÄÍ¾Ã
            tool.Use();

            //ÏûºÄ15·ÖÖÓ
            TimeManager.Instance.AddTime(Events[0].GetTimeEffect());
        });
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        if (card.CardId == "¸Ö²ù")
        {
            tip = Events[0].name;
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        DigByTool(slot.PeekCard(), out tip);
    }
}
