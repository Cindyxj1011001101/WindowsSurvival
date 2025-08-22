using UnityEngine.Events;

public abstract class ToolCard : Card
{
    public override void Use(int times = 1, UnityAction onBroken = null)
    {
        onBroken += () =>
        {
            ShowTip($"{CardName}损坏了");
        };
        base.Use(times, onBroken);
    }
}