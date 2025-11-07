using Newtonsoft.Json;

public class PlantCard : Card
{
    [JsonIgnore] public bool IsRipe => plantGrowth.IsRipe;

    protected virtual void UpdatePlantState() { }

    public void SetPlantGrowth(float value)
    {
        plantGrowth.SetValue(value);
        UpdatePlantState();
    }
}