public class BehaviourExtraEffectsData
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
}