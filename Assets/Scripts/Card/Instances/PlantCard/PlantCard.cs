using Newtonsoft.Json;

public class PlantCard : Card
{
    protected PlantGrowthComponent plantGrowth;

    [JsonIgnore] public bool IsRipe => plantGrowth.IsRipe;

    public override void LateConstrcutor()
    {
        base.LateConstrcutor();
        TryGetComponent(out plantGrowth);
    }

    protected virtual void UpdatePlantState() { }

    public void SetPlantGrowth(float value)
    {
        plantGrowth.SetValue(value);
        UpdatePlantState();
    }
}