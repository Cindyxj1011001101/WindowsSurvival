public abstract class EntityCard : Card, IEntity
{
    private EntityComponent entity;
    private CoordinateComponent coordinate;

    public Coordinate Coordinate => coordinate.coordinate;

    public void TakeDamage(float damage, IEntity damageDealer) => entity.TakeDamage(damage, damageDealer);

    public override void Awake()
    {
        base.Awake();

        TryGetComponent(out entity);
        if (!TryGetComponent(out coordinate))
        {
            coordinate = new();
            AddComponent(coordinate);
        }
    }

    protected override void Start()
    {
        EventManager.Instance.AddListener(EventType.PlayerMove, RefreshSlot);
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.PlayerMove, RefreshSlot);
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        if (card.TryGetComponent<WeaponComponent>(out var weapon) && weapon.WithinAttackRange(this))
        {
            tip = $"攻击该单位\n耗时:  {weapon.attackTime}分钟\n造成伤害:  {weapon.atk}";
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        tip = string.Empty;
        var card = slot.PeekCard();
        card.TryGetComponent<WeaponComponent>(out var weapon);
        weapon.DealDamage(this);
    }
}
