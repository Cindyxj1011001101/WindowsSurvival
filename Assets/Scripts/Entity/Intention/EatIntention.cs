using Newtonsoft.Json;
using System.Collections;
using System.Text;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 进食意图
/// </summary>
public class EatIntention : EntityIntention
{
    [JsonProperty] private string targetUuid;
    
    [JsonIgnore] private Vector3? savedOriginalPosition; // 保存的原始位置，用于返回动效
    [JsonIgnore] private CardSlot tempEntitySlot; // 临时卡槽，用于显示实体移动动效
    [JsonIgnore] private CardSlot originalEntitySlot; // 原卡槽，用于恢复显示

    public EatIntention(int preparationMinutes, string targetUuid) : base(preparationMinutes)
    {
        this.targetUuid = targetUuid;
    }

    public override string GiveName()
    {
        return "进食";
    }

    protected override bool CanExecute()
    {
        var toEat = GlobalDataManager.Instance.GetCardByUuid(targetUuid);
        
        // 1. 食物已不存在，意图执行失败
        if (toEat == null) return false;
        
        // 2. 必须是环境背包中的卡牌
        if (!(toEat.Bag is EnvironmentBag)) return false;
        
        // 3. 如果是作物，必须已成熟
        if (toEat is PlantCard plant)
        {
            if (plant.TryGetComponent<PlantGrowthComponent>(out var plantGrowth))
            {
                if (!plantGrowth.IsRipe) return false; // 未成熟，执行失败
            }
        }
        
        return true;
    }

    public override void OnExecute()
    {
        var toEat = GlobalDataManager.Instance.GetCardByUuid(targetUuid);
        
        // 再次检查执行条件（可能在准备期间状态改变）
        if (!CanExecute())
        {
            // 执行失败，显示失败提示（从实体处弹出）
            ShowExecutionFailedTip(toEat);
            // 失败时立即标记执行完成，让基类可以刷新意图
            ExecuteOver();
            return;
        }

        bool isPlant = toEat is PlantCard;
        
        // 如果是作物，先执行采摘动效
        // 注意：不在这里调用ExecuteOver()，让协程在完成时调用，确保卡牌销毁后再刷新意图
        if (isPlant)
        {
            PublicMono.Instance.StartCoroutine(PlayPickAndEatAnimation(toEat));
        }
        else
        {
            // 普通食物，直接播放食用动效
            PublicMono.Instance.StartCoroutine(PlayEatAnimation(toEat));
        }
    }

    /// <summary>
    /// 显示意图执行失败的提示
    /// </summary>
    private void ShowExecutionFailedTip(Card toEat)
    {
        // 优先使用SlotTransform获取实体在背包中的真实位置，不受详情窗口影响
        var entityTransform = belongedEntity.SlotTransform ?? belongedEntity.Transform;
        if (entityTransform == null) return;

        string tip = "执行失败";
        
        if (toEat == null)
        {
            tip = "目标已丢失";
        }
        else if (!(toEat.Bag is EnvironmentBag))
        {
            tip = "目标不在环境中";
        }
        else if (toEat is PlantCard plant)
        {
            if (plant.TryGetComponent<PlantGrowthComponent>(out var plantGrowth) && !plantGrowth.IsRipe)
            {
                tip = "作物未成熟";
            }
        }

        AnimationManager.Instance.ShowFloatingTipAbove(entityTransform, tip, 0.5f);
    }

    /// <summary>
    /// 播放动效（作物）
    /// </summary>
    private IEnumerator PlayPickAndEatAnimation(Card toEat)
    {
        var plant = toEat as PlantCard;
        // 使用SlotTransform获取卡牌在背包中的真实位置，不受详情窗口影响
        var cardSlotTransform = toEat.SlotTransform;
        
        if (cardSlotTransform == null)
        {
            // 如果卡牌没有SlotTransform，直接执行逻辑
            plant.AddPlantGrowth(-100);
            SoundManager.Instance.PlaySound("采摘植物或采摘果子的音效", true);
            SoundManager.Instance.PlaySound("吃_01", true);
            // 标记执行完成，让基类可以刷新意图（此时卡牌已销毁）
            ExecuteOver();
            yield break;
        }

        var targetPosition = cardSlotTransform.position;

        // 1. 实体移动到目标位置（视觉效果）
        yield return PlayEntityMoveToTarget(targetPosition);

        // 2. 播放采摘音效
        SoundManager.Instance.PlaySound("采摘植物或采摘果子的音效", true);
        
        // 3. 播放采摘动效：轻微弹跳+抖动（使用SlotTransform）
        var pickSeq = DOTween.Sequence();
        pickSeq.Join(AnimationManager.Instance.PlayBounce(cardSlotTransform, 1.08f, 0.2f));
        pickSeq.Join(AnimationManager.Instance.PlayPunch(cardSlotTransform, 0.4f));
        
        yield return pickSeq.WaitForCompletion();
        
        // 4. 播放食用音效
        SoundManager.Instance.PlaySound("吃_01", true);
        
        // 5. 显示"食用"浮动提示（从实体处弹出，使用真实位置，只显示一个提示）
        var entityTransform = belongedEntity.SlotTransform ?? belongedEntity.Transform;
        if (entityTransform != null)
        {
            AnimationManager.Instance.ShowFloatingTipAbove(entityTransform, "食用", 0.5f);
        }
        
        // 7. 实体返回原位（视觉效果）
        yield return PlayEntityReturnToOriginal();
        
        // 8. 执行实际逻辑：减少植物生长度（不播放销毁动效，避免影响卡槽）
        plant.AddPlantGrowth(-100);
        
        // 9. 标记执行完成，让基类可以刷新意图（此时卡牌已销毁）
        ExecuteOver();
    }

    /// <summary>
    /// 播放食用动效（普通食物）
    /// </summary>
    private IEnumerator PlayEatAnimation(Card toEat)
    {
        // 使用SlotTransform获取卡牌在背包中的真实位置，不受详情窗口影响
        var cardSlotTransform = toEat.SlotTransform;
        
        if (cardSlotTransform == null)
        {
            // 如果卡牌没有SlotTransform，直接执行逻辑
            toEat.DestroyThis();
            SoundManager.Instance.PlaySound("吃_01", true);
            // 标记执行完成，让基类可以刷新意图（此时卡牌已销毁）
            ExecuteOver();
            yield break;
        }

        var targetPosition = cardSlotTransform.position;

        // 1. 实体移动到目标位置（视觉效果）
        yield return PlayEntityMoveToTarget(targetPosition);

        // 2. 播放食用音效
        SoundManager.Instance.PlaySound("吃_01", true);
        
        // 3. 显示"食用"浮动提示（从实体处弹出，使用真实位置）
        var entityTransform = belongedEntity.SlotTransform ?? belongedEntity.Transform;
        if (entityTransform != null)
        {
            AnimationManager.Instance.ShowFloatingTipAbove(entityTransform, "食用", 0.5f);
        }
        
        // 4. 实体返回原位（视觉效果）
        yield return PlayEntityReturnToOriginal();
        
        // 5. 执行实际逻辑：销毁卡牌（不播放销毁动效，避免影响卡槽）
        toEat.DestroyThis();
        
        // 6. 标记执行完成，让基类可以刷新意图（此时卡牌已销毁）
        ExecuteOver();
    }

    /// <summary>
    /// 播放实体移动到目标位置的动效（仅视觉效果，不影响实际位置）
    /// 使用AnimationManager的统一动效方法
    /// </summary>
    private IEnumerator PlayEntityMoveToTarget(Vector3 targetPosition)
    {
        var entityTransform = belongedEntity.Transform;
        if (entityTransform == null) yield break;

        // 保存原始位置（从SlotTransform获取，确保是真实位置）
        savedOriginalPosition = belongedEntity.SlotTransform != null 
            ? belongedEntity.SlotTransform.position 
            : entityTransform.position;
        
        // 使用AnimationManager的统一动效方法
        var (tempSlot, moveTween, originalSlot) = AnimationManager.Instance.PlayEntityMoveToTarget(
            belongedEntity, 
            targetPosition
        );
        tempEntitySlot = tempSlot;
        originalEntitySlot = originalSlot;
        
        // 等待移动完成
        yield return moveTween.WaitForCompletion();
    }

    /// <summary>
    /// 播放实体返回原位的动效（仅视觉效果，不影响实际位置）
    /// 使用AnimationManager的统一动效方法
    /// </summary>
    private IEnumerator PlayEntityReturnToOriginal()
    {
        if (tempEntitySlot == null || !savedOriginalPosition.HasValue) yield break;

        // 使用AnimationManager的统一动效方法
        var returnTween = AnimationManager.Instance.PlayEntityReturnToOriginal(
            tempEntitySlot,
            savedOriginalPosition.Value,
            originalEntitySlot,
            onComplete: () =>
            {
                tempEntitySlot = null;
                originalEntitySlot = null;
                savedOriginalPosition = null;
            }
        );
        
        // 等待返回完成
        yield return returnTween.WaitForCompletion();
    }

    public override string GetDescription()
    {
        var toEat = GlobalDataManager.Instance.GetCardByUuid(targetUuid);

        var sb = new StringBuilder();
        
        // 检查各种失败条件
        if (toEat == null)
        {
            sb.AppendLine($"食用目标:  已丢失");
            return sb.ToString();
        }
        
        sb.AppendLine($"食用目标:  {toEat.CardName}");
        
        // 如果是植物，显示成熟状态
        if (toEat is PlantCard plant)
        {
            if (plant.TryGetComponent<PlantGrowthComponent>(out var plantGrowth))
            {
                if (plantGrowth.IsRipe)
                    sb.AppendLine($"成熟状态:  已成熟");
                else
                    sb.AppendLine($"成熟状态:  未成熟（无法食用）");
            }
        }

        // TODO: 策划配置的描述文本

        return sb.ToString();
    }
}