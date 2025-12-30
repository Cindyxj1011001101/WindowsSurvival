using Newtonsoft.Json;

public abstract class SingleTargetIntention : EntityIntention
{
    [JsonProperty] protected string targetUuid;

    [JsonIgnore] protected IEntity EntityTarget => GlobalDataManager.Instance.GetEntityByUuid(targetUuid);

    [JsonIgnore] protected Card CardTarget => GlobalDataManager.Instance.GetCardByUuid(targetUuid);

    protected SingleTargetIntention(int preparationMinutes, string targetUuid) : base(preparationMinutes)
    {
        this.targetUuid = targetUuid;
    }
}