using Newtonsoft.Json;

public abstract class SingleTargetIntention : EntityIntention
{
    [JsonProperty] protected string targetUuid;
    [JsonIgnore] private IEntity entityTarget;
    [JsonIgnore] private Card cardTarget;

    [JsonIgnore] protected IEntity EntityTarget
    {
        get
        {
            entityTarget ??= GlobalDataManager.Instance.GetEntityByUuid(targetUuid);
            return entityTarget;
        }
    }

    [JsonIgnore] protected Card CardTarget
    {
        get
        {
            cardTarget ??= GlobalDataManager.Instance.GetCardByUuid(targetUuid);
            return cardTarget;
        }
    }

    protected SingleTargetIntention(int preparationMinutes, string targetUuid) : base(preparationMinutes)
    {
        this.targetUuid = targetUuid;
        entityTarget = GlobalDataManager.Instance.GetEntityByUuid(targetUuid);
        cardTarget = GlobalDataManager.Instance.GetCardByUuid(targetUuid);
    }
}