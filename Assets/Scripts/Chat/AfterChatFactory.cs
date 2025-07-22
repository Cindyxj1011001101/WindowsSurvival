using System.Collections.Generic;

public class AfterChatFactory
{
    public void Init(string EventName)
    {
        //根据EventName创建对应的事件;英文分号隔开两个事件
        //音效：音效_音效名_是否随机
        //音乐：音乐_音乐名_是否循环
        //状态：状态_目标状态位置（玩家/当前环境/维生舱/驾驶室/动力舱/珊瑚礁海域）_状态名(健康/饱食/口渴/精神/氧气/疲劳/电力/氧气/压力/高度/电缆/水域)_数值
        //时间：时间_数值
        //其他：其他_其他名
        List<string> eventList = new List<string>(EventName.Split(';'));
        List<List<string>> eventListList = new List<List<string>>();
        foreach (string eventItem in eventList)
        {
            List<string> eventItemList = new List<string>(eventItem.Split('_'));
            eventListList.Add(eventItemList);
        }
        foreach (List<string> eventItemList in eventListList)
        {
            switch (eventItemList[0])
            {
                case "音效":
                    SoundManager.Instance.PlaySound(eventItemList[1], eventItemList[2] == "true");
                    break;
                case "音乐":
                    SoundManager.Instance.PlayBGM(eventItemList[1], eventItemList[2] == "true");
                    break;
                case "状态":
                    ChangeState(eventItemList);
                    break;
                case "时间":
                    TimeManager.Instance.AddTime(int.Parse(eventItemList[1]));
                    break;
                case "其他":
                    OtherEvent(eventItemList);
                    break;
            }
        }
    }
    public void ChangeState(List<string> eventItemList)
    {
        switch (eventItemList[1])
        {
            case "玩家":
                StateManager.Instance.ChangePlayerStateByString(eventItemList[2], float.Parse(eventItemList[3]));
                break;
            case "当前环境":
                StateManager.Instance.ChangeEnvironmentStateByString(GameManager.Instance.CurEnvironmentBag.PlaceData.placeType, eventItemList[2], float.Parse(eventItemList[3]));
                break;
            case "维生舱":
                StateManager.Instance.ChangeEnvironmentStateByString(PlaceEnum.LifeSupportCabin, eventItemList[2], float.Parse(eventItemList[3]));
                break;
            case "驾驶室":
                StateManager.Instance.ChangeEnvironmentStateByString(PlaceEnum.Cockpit, eventItemList[2], float.Parse(eventItemList[3]));
                break;
            case "动力舱":
                StateManager.Instance.ChangeEnvironmentStateByString(PlaceEnum.PowerCabin, eventItemList[2], float.Parse(eventItemList[3]));
                break;
            case "珊瑚礁海域":
                StateManager.Instance.ChangeEnvironmentStateByString(PlaceEnum.CoralCoast, eventItemList[2], float.Parse(eventItemList[3]));
                break;
        }

    }
    public void OtherEvent(List<string> eventItemList)
    {
        switch (eventItemList[1])
        {
            case "":
                break;
        }
    }
}