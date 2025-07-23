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
                ChangePlayerStateByString(eventItemList[2], float.Parse(eventItemList[3]));
                break;
            case "当前环境":
                ChangeEnvironmentStateByString(GameManager.Instance.CurEnvironmentBag.PlaceData.placeType, eventItemList[2], float.Parse(eventItemList[3]));
                break;
            case "维生舱":
                ChangeEnvironmentStateByString(PlaceEnum.LifeSupportCabin, eventItemList[2], float.Parse(eventItemList[3]));
                break;
            case "驾驶室":
                ChangeEnvironmentStateByString(PlaceEnum.Cockpit, eventItemList[2], float.Parse(eventItemList[3]));
                break;
            case "动力舱":
                ChangeEnvironmentStateByString(PlaceEnum.PowerCabin, eventItemList[2], float.Parse(eventItemList[3]));
                break;
            case "珊瑚礁海域":
                ChangeEnvironmentStateByString(PlaceEnum.CoralCoast, eventItemList[2], float.Parse(eventItemList[3]));
                break;
        }

    }

    private void ChangePlayerStateByString(string stateName, float delta)
    {
        switch (stateName)
        {
            case "健康":
                StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, delta);
                break;
            case "饱食":
                StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, delta);
                break;
            case "口渴":
                StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, delta);
                break;
            case "精神":
                StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, delta);
                break;
            case "氧气":
                StateManager.Instance.ChangePlayerState(PlayerStateEnum.Oxygen, delta);
                break;
            case "清醒":
                StateManager.Instance.ChangePlayerState(PlayerStateEnum.Sobriety, delta);
                break;
        }
    }

    private void ChangeEnvironmentStateByString(PlaceEnum placeType, string stateName, float delta)
    {
        var env = GameManager.Instance.EnvironmentBags[placeType];
        switch (stateName)
        {
            case "电力":
                StateManager.Instance.ChangeElectricity(delta);
                break;
            case "氧气":
                env.ChangeEnvironmentState(EnvironmentStateEnum.Oxygen, delta);
                break;
            case "压力":
                env.ChangeEnvironmentState(EnvironmentStateEnum.Oxygen, delta);
                break;
            case "高度":
                StateManager.Instance.ChangeWaterLevel(delta);
                break;
            //case "电缆":
            //    OnEnvironmentChangeState(new ChangeEnvironmentStateArgs(placeType, EnvironmentStateEnum.HasCable, delta));
            //    break;
            //case "水域":
            //    OnEnvironmentChangeState(new ChangeEnvironmentStateArgs(placeType, EnvironmentStateEnum.InWater, delta));
            //    break;
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