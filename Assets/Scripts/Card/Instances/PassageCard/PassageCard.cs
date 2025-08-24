public abstract class PassageCard : Card
{
    private PassageComponent passage;
    protected PassageCard()
    {
        Events = new()
        {
            new Event("前往", "", Event_Enter, Judge_Enter),
        };
    }

    public override void LateInit()
    {
        base.LateInit();
        TryGetComponent(out passage);

        EventManager.Instance.AddListener<PlayerStateEnum>(EventType.RefreshPlayerState, OnLoadChange);

        Events[0].description = GameManager.Instance.GetMoveEffects(passage.time, passage.targetPlace).desc;
        Events[0].getTimeEffect = () => GameManager.Instance.GetMoveEffects(passage.time, passage.targetPlace).time;
        Events[0].getPlayerEffects = () => GameManager.Instance.GetMoveEffects(passage.time, passage.targetPlace).playerEffects;
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

    public override void DestroyThis()
    {
        base.DestroyThis();
        EventManager.Instance.RemoveListener<PlayerStateEnum>(EventType.RefreshPlayerState, OnLoadChange);
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
        if (!GameManager.Instance.CanMoveExplore())
        {
            hint = "身上太重了，无法前往";
            return false;
        }
        return true;
    }
}