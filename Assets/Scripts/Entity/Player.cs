using Newtonsoft.Json;
using System.Collections.Generic;

public class Player : IEntity
{
    [JsonProperty] private float basicMoveDistPerMin = 0.5f;

    [JsonProperty] private List<float> moveSpeedMultiplier = new();

    [JsonIgnore]
    public float MoveSpeed
    {
        get
        {
            var speed = basicMoveDistPerMin;
            foreach (var m in moveSpeedMultiplier)
            {
                speed *= 1 + m;
            }
            return speed;
        }
    }

    public Coordinate Coordinate { get; private set; } = new();

    public void TakeDamage(float damage, IEntity damageDealer)
    {
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -damage);
    }

    public void AddMoveSpeedMultiplier(float multipier) => moveSpeedMultiplier.Add(multipier);

    public void RemoveMoveSpeedMultiplier(float multipier) => moveSpeedMultiplier.Remove(multipier);
}