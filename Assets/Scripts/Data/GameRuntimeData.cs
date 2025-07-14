using System;
using System.Collections.Generic;

public class GameRuntimeData
{
    //时间数据   
    public DateTime CurTime;
    public int CurInterval;
    //玩家状态数据
    public Dictionary<PlayerStateEnum, PlayerState> PlayerStateDict;
    //装备数据
    public CardInstance HeadEquipment;
    public CardInstance BodyEquipment;
    public CardInstance BackEquipment;
    public CardInstance LegEquipment;
}