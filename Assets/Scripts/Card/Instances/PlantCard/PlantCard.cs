using Newtonsoft.Json;

public abstract class PlantCard : Card
{
    [JsonIgnore] public bool IsRipe => plantGrowth.IsRipe;

    protected virtual void UpdatePlantState() { }

    public void AddPlantGrowth(float delta)
    {
        plantGrowth.AddValue(delta);
        UpdatePlantState();
    }
}