public class Player : IEntity
{
    public float moveDistPerMin = 1;

    public Coordinate Coordinate { get; private set; } = new();

    public void TakeDamage(float damage, IEntity damageDealer)
    {
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -damage);
    }
}