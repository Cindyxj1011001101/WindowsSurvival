using UnityEngine.Events;

public abstract class PlantCard : Card
{
    protected override System.Action OnUpdate => () =>
    {
        base.OnUpdate();
        if (TryGetComponent<PlantGrowthComponent>(out var component))
        {
            component.Grow((EnvironmentBag)Bag);
        }

        if (component.DeadProgress == 5)
        {
            DestroyThis();
            AddCard(component.DeadCardName,true);
        }
    };
}