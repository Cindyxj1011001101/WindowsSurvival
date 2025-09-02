public class BehaviourExtraEffectsData:VersionMigrator
{
    public bool init;

    // 探索额外消耗
    public BehaviourExtraEffects exploreExtraEffects = new();

    // 探索水域额外消耗
    public BehaviourExtraEffects exploreInWaterExtraEffects = new();

    // 移动额外消耗
    public BehaviourExtraEffects moveExtraEffects = new();

    // 移动到水域额外消耗
    public BehaviourExtraEffects moveToWaterExtraEffects = new();

    public override IVersionMigrator ReadJSON(string FilePath, string FileName)
    {
        return JsonManager.LoadData<BehaviourExtraEffectsData>(FilePath,FileName);
    }
}