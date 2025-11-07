public abstract class PassageCard : Card
{
    private const float MAX_AVAILABLE_DIST = 3.0f; // 小于等于该距离时可以使用通道

    protected override void RegisterCardEvents()
    {
        AddCardEvent("通过",
            () => "前往" + ParsePlaceEnum(passage.targetPlace) + MoveExploreManager.Instance.GetMoveEffects(passage.time, passage.targetPlace).desc,
            Event_Enter, Judge_Enter,
            () => MoveExploreManager.Instance.GetMoveEffects(passage.time, passage.targetPlace).time,
            () => MoveExploreManager.Instance.GetMoveEffects(passage.time, passage.targetPlace).playerStateChanges);
        AddCardEvent("移至附近",
            () => "移动到通道的附近" + MoveExploreManager.Instance.GetMoveEffects(GetNearestAvailablePosition()).desc,
            Event_MoveNear, Judge_MoveNear,
            () => MoveExploreManager.Instance.GetMoveEffects(GetNearestAvailablePosition()).time,
            () => MoveExploreManager.Instance.GetMoveEffects(GetNearestAvailablePosition()).playerStateChanges);
    }

    protected override void OnInit()
    {
        EventManager.Instance.AddListener<PlayerStateEnum>(EventType.RefreshPlayerState, OnLoadChange);
        EventManager.Instance.AddListener(EventType.PlayerMove, RefreshSlot);
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener<PlayerStateEnum>(EventType.RefreshPlayerState, OnLoadChange);
        EventManager.Instance.RemoveListener(EventType.PlayerMove, RefreshSlot);
    }

    /// <summary>
    /// 载重变化时刷新卡槽
    /// </summary>
    /// <param name="state"></param>
    private void OnLoadChange(PlayerStateEnum state)
    {
        if (state == PlayerStateEnum.Load)
        {
            RefreshSlot();
        }
    }

    private string ParsePlaceEnum(PlaceEnum place)
    {
        return GameManager.Instance.PlaceDataDict[place].placeName;
    }

    protected virtual void Event_Enter(out string tip)
    {
        tip = string.Empty;
        PlaySound(passage.audioClip, true);
        MoveExploreManager.Instance.Move(passage.targetPlace, passage.time);
    }

    protected virtual bool Judge_Enter(out string hint)
    {
        hint = string.Empty;
        if (!IsPlayerNear())
        {
            hint = "距离太远，无法通过";
            return false;
        }

        if (!MoveExploreManager.Instance.CanMoveExplore())
        {
            hint = "身上太重了，无法通过";
            return false;
        }
        return true;
    }

    protected virtual void Event_MoveNear(out string tip)
    {
        tip = string.Empty;
        // 移动到最近的可用使用通道的坐标
        MoveExploreManager.Instance.Move(GetNearestAvailablePosition());
    }

    protected virtual bool Judge_MoveNear(out string hint)
    {
        hint = string.Empty;

        if (IsPlayerNear())
        {
            hint = "已经在附近了，无需移动";
            return false;
        }

        if (!MoveExploreManager.Instance.CanMoveExplore())
        {
            hint = "身上太重了，无法移动";
            return false;
        }

        return true;
    }

    private bool IsPlayerNear()
    {
        return Player.Instance.Coordinate.DistanceTo(coordinate.coordinate) <= MAX_AVAILABLE_DIST;
    }

    private float GetNearestAvailablePosition()
    {
        var playerPos = Player.Instance.Coordinate.Position;
        var passagePos = coordinate.coordinate.Position;
        return playerPos > passagePos ? passagePos + MAX_AVAILABLE_DIST : passagePos - MAX_AVAILABLE_DIST;
    }
}