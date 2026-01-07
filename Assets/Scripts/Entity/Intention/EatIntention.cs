using System.Text;

/// <summary>
/// 进食意图
/// </summary>
public class EatIntention : SingleTargetIntention
{
    protected override bool WithoutAnim => false;

    public EatIntention(int preparationMinutes, string targetUuid) : base(preparationMinutes, targetUuid)
    {
    }

    public override string GiveName()
    {
        return "进食";
    }

    protected override bool CanExecute()
    {
        // 食物已不存在，意图执行失败
        return CardTarget != null && belongedEntity.IsInSameBag(CardTarget);
    }

    public override void OnExecute()
    {
        var sourceSlot = belongedEntity.Slot;
        var tempSlot = AnimationManager.Instance.CreateSlotCopy(belongedEntity);
        if (tempSlot != null)
        {
            AnimationManager.Instance.PlayEatIntentionEffect(
                CardTarget,
                tempSlot,
                OnBite,
                OnComplete);
        }
        else
        {
            OnComplete();
        }

        void OnBite()
        {
            if (CardTarget is PlantCard plant)
            {
                plant.AddPlantGrowth(-100);
                SoundManager.Instance.PlaySound("采摘植物或采摘果子的音效", true);
            }
            else
            {
                CardTarget.DestroyThis();
                SoundManager.Instance.PlaySound("吃_01", true);
            }
        }

        void OnComplete()
        {
            if (sourceSlot != null)
                sourceSlot.DontRefresh = false;
            ExecuteOver();
        }
    }

    public override string GetDescription()
    {
        // 食物已不存在，意图执行失败
        var targetLoss = CardTarget == null || !belongedEntity.IsInSameBag(CardTarget);

        var sb = new StringBuilder();
        if (targetLoss)
            sb.AppendLine($"食用目标:  {ColorManager.Colorize("已丢失", ColorManager.LightGrey)}");
        else
            sb.AppendLine($"食用目标:  {ColorManager.Colorize(CardTarget.CardName, ColorManager.Yellow)}");

        // TODO: 策划配置的描述文本

        return sb.ToString();
    }
}