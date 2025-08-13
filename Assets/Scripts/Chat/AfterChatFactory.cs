using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public static class AfterChatFactory
{
    public static void TriggerEffect(string EventName)
    {
        //根据EventName创建对应的事件;英文分号隔开两个事件
        //音效：音效_音效名_是否随机
        //音乐：音乐_音乐名_是否循环
        //状态：状态_目标状态位置（玩家/当前环境/维生舱/驾驶室/动力舱/珊瑚礁海域）_状态名(健康/饱食/口渴/精神/氧气/疲劳/电力/氧气/压力/高度/电缆/水域)_数值
        //时间：时间_数值
        //解锁：解锁_目标窗口名称
        //添加：添加_玩家（玩家/场景）_压缩饼干（物品名称）
        //其他：其他_其他名
        if(EventName=="")return;
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
                case "解锁":
                    UnlockWindow(eventItemList[1], eventItemList[2]);
                    break;
                case "添加":
                    AddCardEvent(eventItemList[1], eventItemList[2]); 
                    break;
                case "其他":
                    OtherEvent(eventItemList);
                    break;
            }
        }
    }

    private static void AddCardEvent(string PlayerOrScene, string CardName)
    {
        GameManager.Instance.AddCardWithTween(CardName, new Vector2(0,-700), PlayerOrScene=="玩家");
    }

    private static void UnlockWindow(string WindowName,string blink)
    {
        bool Addblink = blink == "true" ? true : false;
        string windowName = WindowName switch
        {
            "背包" => "PlayerBag",
            "摄像头"=>"Camera",
            "状态" => "State",
            "研究" => "Study",
            "地点" => "EnvironmentBag",
            "休息" => "Rest",
            "装备" => "Equipment",
            "制作"=>"Craft",
            "详情"=>"Details",
            _ => throw new System.Exception("WindowName Error")
        };
        WindowsManager.Instance.UnlockShortcut(windowName,Addblink);
        //解锁窗口逻辑
    }

    public static void ChangeState(List<string> eventItemList)
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

    private static void ChangePlayerStateByString(string stateName, float delta)
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

    private static void ChangeEnvironmentStateByString(PlaceEnum placeType, string stateName, float delta)
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

    public static void OtherEvent(List<string> eventItemList)
    {
        switch (eventItemList[1])
        {
            case "死亡":
                Die();
                break;
        }
    }
    public static void Die()
    {
        ChatManager.Instance.ParagraphToTriggeer.Clear();
        ChatManager.Instance.InterruptParagraphData=null;
        // 延迟1秒执行删除存档和返回主菜单的操作
        ChatManager.Instance.Invoke(nameof(ChatManager.ReturnToMainMenuAndDeleteSave), 2f);

    }
}