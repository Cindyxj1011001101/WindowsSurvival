/// <summary>
/// 吸盘蠕虫
/// </summary>
public class SuckerWorm : EntityCard
{
    protected override void OnLateConstructor()
    {
        // 自带对玩家的永久仇恨

    }

    protected override string GetHighestPriorityIntention()
    {
        var aggro = GetAggroTarget();

        // 没有仇恨目标
        if (aggro == null)
        {

        }

        // 仇恨优先级 >= 7
        if (aggro != null && aggro.Priority >= 7)
        {

        }

        return null;
    }

    protected override void RegisterIntentions()
    {
        AddIntention("移动", 5, Intention_Move);
        AddIntention("攻击", 5, Intention_Attack);
        AddIntention("食用", 15, Intention_Eat);
        AddIntention("寻找麦麦", 5, Intention_FindThePlayer);
    }

    private void Intention_Move()
    {

    }

    private void Intention_Attack()
    {

    }

    private void Intention_Eat()
    {

    }

    private void Intention_FindThePlayer()
    {

    }
}
