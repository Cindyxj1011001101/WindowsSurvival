public abstract class PassageCard : Card
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
        base.LateInit();
        TryGetComponent<PassageComponent>(out var component);
        Events[0].description = "前往" + GameManager.ParsePlaceEnum(component.targetPlace);
        Events[0].getTimeEffect = () => component.time + GameManager.Instance.GetExtraMoveExploreTime(component.time);
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
        hint = GameManager.Instance.GetMoveDesc(Events[0].description);
        return GameManager.Instance.CanMoveExplore();
    }
}