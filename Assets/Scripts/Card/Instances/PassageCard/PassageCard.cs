public class PassageCard : Card
{
    protected PassageCard()
    {
        Events = new()
        {
            new Event("前往", "前往", Event_Enter, Judge_Enter, null,
                () => GameManager.Instance.GetMoveExplorePlayerEffects(), null),
        };
    }

    protected override void LateInit()
    {
        TryGetComponent<PassageComponent>(out var component);
        Events[0].description = "前往" + GameManager.ParsePlaceEnum(component.targetPlace);
        Events[0].getTimeEffect = () => GameManager.Instance.GetMoveExploreTime(component.time);
    }

    public virtual void Event_Enter(out string tip)
    {
        tip = string.Empty;
        TryGetComponent<PassageComponent>(out var component);
        if (!string.IsNullOrEmpty(component.audioClip))
            SoundManager.Instance.PlaySound(component.audioClip, true);
        GameManager.Instance.Move(component.targetPlace, component.time);
    }

    public virtual bool Judge_Enter(out string hint)
    {
        hint = GetMoveDesc(Events[0].description);
        return GameManager.Instance.CanMoveExplore();
    }

    private string GetMoveDesc(string origin)
    {
        string result = origin;
        int level = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Load].StateLevel;
        switch (level)
        {
            case 0:
                break;
            case 1:
                result += "\n身上有点重，额外消耗25%时间";
                break;
            case 2:
                result += "\n身上很重，额外消耗100%时间";
                break;
            case 3:
                result = "身上太重了，没法这么做";
                break;
        }
        return result;
    }
}