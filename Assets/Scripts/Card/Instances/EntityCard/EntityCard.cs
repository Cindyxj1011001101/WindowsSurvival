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

    public override void OnAdd(Bag bag)
    {
        base.OnAdd(bag);

        var env = bag as EnvironmentBag;
        // 将自身添加到地点的实体列表中
        env.AddEntity(this);
    }

    public override void OnRemove(Bag bag)
    {
        base.OnRemove(bag);

        var env = bag as EnvironmentBag;
        // 将自身从地点的实体列表中移除
        env.RemoveEntity(this);
    }
}
