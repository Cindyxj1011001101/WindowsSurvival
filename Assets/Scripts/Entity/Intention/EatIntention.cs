using System.Text;

/// <summary>
/// 进食意图
/// </summary>
public class EatIntention : SingleTargetIntention
{
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
        if (CardTarget is PlantCard plant)
        {
            plant.AddPlantGrowth(-100);
            SoundManager.Instance.PlaySound("采摘植物或采摘果子的音效", true);
        }  
        else
        {
            CardTarget.DestroyThis();
        }
        SoundManager.Instance.PlaySound("吃_01", true);  

        // TODO: 吃掉动效
    }

    public override string GetDescription()
    {
        // 食物已不存在，意图执行失败
        var targetLoss = CardTarget == null || !belongedEntity.IsInSameBag(CardTarget);

        var sb = new StringBuilder();
        if (targetLoss)
            sb.AppendLine($"食用目标:  已丢失");
        else
            sb.AppendLine($"食用目标:  {CardTarget.CardName}");

        // TODO: 策划配置的描述文本

        return sb.ToString();
    }
}