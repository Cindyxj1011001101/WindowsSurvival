public class Player : IEntity
{
    public float moveDistPerMin = .5f;

    public Coordinate Coordinate { get; private set; } = new();

    public void TakeDamage(float damage, IEntity damageDealer)
    {
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -damage);
    }
}