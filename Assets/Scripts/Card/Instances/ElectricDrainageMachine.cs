public class ElectricDrainageMachine : Card
{
    private ElectricDrainageMachine()
    {
        Events = new()
        {
            new Event("开启", "开启电动排水机", Event_Open, null),
            new Event("关闭", "关闭电动排水机", Event_Close, null)
        };
    }
    //开启状态下每回合-0.5电，使水面高度-0.8；
    //当每回合结算时水面高度为0或电力不足0.5，本回合就不消耗电力并自动关闭。
    //只能在室内非水域环境建造。
    //交互行为：开启、关闭
    public void Event_Open()
    {
    }
    public void Event_Close()
    {
    }

}