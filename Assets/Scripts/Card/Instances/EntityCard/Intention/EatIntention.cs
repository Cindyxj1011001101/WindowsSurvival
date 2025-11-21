using Newtonsoft.Json;
using System.Text;

/// <summary>
/// 进食意图
/// </summary>
public class EatIntention : EntityIntention
{
    [JsonProperty] private string targetUuid;

    public EatIntention(int preparationMinutes, string targetUuid) : base(preparationMinutes)
    {
        this.targetUuid = targetUuid;
    }

    public override string GiveName()
    {
        return "进食";
    }

    public override bool CanExecute()
    {
        var toEat = GlobalDataManager.Instance.GetCardByUuid(targetUuid);
        // 食物已不存在，意图执行失败
        return toEat != null && belongedEntity.IsInSameBag(toEat);
    }

    public override void Execute()
    {
        var toEat = GlobalDataManager.Instance.GetCardByUuid(targetUuid);

        if (toEat is PlantCard plant)
        {
            plant.AddPlantGrowth(-100);
            SoundManager.Instance.PlaySound("采摘植物或采摘果子的音效", true);
        }  
        else
        {
            toEat.DestroyThis();
        }
        SoundManager.Instance.PlaySound("吃_01", true);  

        // TODO: 吃掉动效
    }

    public override string GetDescription()
    {
        var toEat = GlobalDataManager.Instance.GetCardByUuid(targetUuid);

        // 食物已不存在，意图执行失败
        var targetLoss = toEat == null || !belongedEntity.IsInSameBag(toEat);

        var sb = new StringBuilder();
        if (targetLoss)
            sb.AppendLine($"食用目标:  已丢失");
        else
            sb.AppendLine($"食用目标:  {toEat.CardName}");

        // TODO: 策划配置的描述文本

        return sb.ToString();
    }
}