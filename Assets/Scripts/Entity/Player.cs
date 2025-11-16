using System.Collections.Generic;

public class Player : IEntity, IManager
{
    public static Player Instance { get; } = new();

    public float Atk { get; set; } = 5;
    public int AttackTime { get; set; } = 5;
    public float AttackRange { get; set; } = 1;

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

    public string Uuid => "player";

    public void Init()
    {
        var data = GameDataManager.Instance.PlayerData;
        MoveSpeedMultiplier = data.moveSpeedMultiplier;
        Coordinate = data.coordinate;

        // 加入到实体的全局记录
        GlobalDataManager.Instance.CreateEntity(this);
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

    public void DealDamage(IEntity target)
    {
        // 造成伤害
        target.TakeDamage(Atk, this);
        // 消耗时间
        TimeManager.Instance.AddTime(AttackTime);
    }
    public bool WithinAttackRange(IEntity target)
    {
        var dist = target.DistanceTo(this);
        return dist <= AttackRange;
    }

    public bool CanAttack(IEntity target, out string reason)
    {
        reason = string.Empty;
        if (!WithinAttackRange(target))
        {
            reason = "距离目标太远";
            return false;
        }

        return true;
    }

    public void AddMoveSpeedMultiplier(float multipier) => MoveSpeedMultiplier.Add(multipier);

    public void RemoveMoveSpeedMultiplier(float multipier) => MoveSpeedMultiplier.Remove(multipier);

    public void MoveTo(float targetPosition)
    {
        Coordinate.SetPosition(targetPosition);
        EventManager.Instance.TriggerEvent(EventType.PlayerMove);
    }
}