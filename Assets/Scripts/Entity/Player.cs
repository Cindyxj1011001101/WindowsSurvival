using System.Collections.Generic;

public class Player : IEntity, IManager
{
    public static Player Instance { get; } = new();

    public float BasicMoveDistPerMin { get; private set; } = 0.5f;

    public List<float> MoveSpeedMultiplier { get; private set; } = new();

    public Coordinate Coordinate { get; private set; } = new();

    public float MoveSpeed
    {
        get
        {
            var speed = BasicMoveDistPerMin;
            foreach (var m in MoveSpeedMultiplier)
            {
                speed *= 1 + m;
            }
            return speed;
        }
    }

    public string UUID => "player";

    public void Init()
    {
        var data = GameDataManager.Instance.PlayerData;
        MoveSpeedMultiplier = data.moveSpeedMultiplier;
        Coordinate = data.coordinate;

        // 加入到实体的全局记录
        GlobalDataManager.Instance.AddEntity(this);
    }

    public void Reset()
    {
        MoveSpeedMultiplier = new();
        Coordinate = new();
    }

    public void TakeDamage(float damage, IEntity damageDealer)
    {
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -damage);
        // 中断休息行为
        StateManager.Instance.StopResting();
    }

    public void AddMoveSpeedMultiplier(float multipier) => MoveSpeedMultiplier.Add(multipier);

    public void RemoveMoveSpeedMultiplier(float multipier) => MoveSpeedMultiplier.Remove(multipier);
}