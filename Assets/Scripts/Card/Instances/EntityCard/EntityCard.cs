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
}
