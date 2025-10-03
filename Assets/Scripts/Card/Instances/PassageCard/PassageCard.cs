public abstract class PassageCard : Card
{
    private PassageComponent passage;
    private CoordinateComponent coordinate;

    private const float MaxAvailableDist = 3.0f; // 小于等于该距离时可以使用

    protected PassageCard()
    {
        Events = new()
        {
            new Event("前往", "", Event_Enter, Judge_Enter),
        };
    }

    public override void Awake()
    {
        base.Awake();
        TryGetComponent(out coordinate);
        TryGetComponent(out passage);

        Events[0].description = GameManager.Instance.GetMoveEffects(passage.time, passage.targetPlace).desc;
        Events[0].getTimeEffect = () => GameManager.Instance.GetMoveEffects(passage.time, passage.targetPlace).time;
        Events[0].getPlayerEffects = () => GameManager.Instance.GetMoveEffects(passage.time, passage.targetPlace).playerEffects;
    }

    protected override void Start()
    {
        EventManager.Instance.AddListener<PlayerStateEnum>(EventType.RefreshPlayerState, OnLoadChange);
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener<PlayerStateEnum>(EventType.RefreshPlayerState, OnLoadChange);
    }

    /// <summary>
    /// 载重变化时刷新卡槽
    /// </summary>
    /// <param name="state"></param>
    private void OnLoadChange(PlayerStateEnum state)
    {
        if (state == PlayerStateEnum.Load)
        {
            Events[0].description = GameManager.Instance.GetMoveEffects(passage.time, passage.targetPlace).desc;
            RefreshSlot();
        }
    }

    public virtual void Event_Enter(out string tip)
    {
        tip = string.Empty;
        if (!string.IsNullOrEmpty(passage.audioClip))
            SoundManager.Instance.PlaySound(passage.audioClip, true);
        GameManager.Instance.Move(passage.targetPlace, passage.time);
    }

    public virtual bool Judge_Enter(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.Player.Coordinate.DistanceTo(coordinate.coordinate) > MaxAvailableDist)
        {
            hint = "距离通道太远，无法前往";
            return false;
        }

        if (!GameManager.Instance.CanMoveExplore())
        {
            hint = "身上太重了，无法前往";
            return false;
        }
        return true;
    }
}