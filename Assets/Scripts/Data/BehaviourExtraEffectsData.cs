public class BehaviourExtraEffectsData
{
    public bool init;

    // 探索额外消耗
    public MoveExploreExtraEffects exploreExtraEffects = new();

    // 探索水域额外消耗
    public MoveExploreExtraEffects exploreInWaterExtraEffects = new();

    // 移动额外消耗
    public MoveExploreExtraEffects moveExtraEffects = new();

    // 移动到水域额外消耗
    public MoveExploreExtraEffects moveToWaterExtraEffects = new();
}