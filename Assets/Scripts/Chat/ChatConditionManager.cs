using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ChatConditionManager : MonoBehaviour
{
    public static ChatConditionManager Instance { get; private set; }
    public List<ParagraphData> ParagraphConditionsToTrigger = new List<ParagraphData>();
    public Dictionary<string, ChatCondition> DetectedChatConditions = new Dictionary<string, ChatCondition>();
    public Dictionary<string, ParagraphCondition> DetectedParagraphConditions = new Dictionary<string, ParagraphCondition>();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    
        Instance = this;
        EventManager.Instance.AddListener<SubscribeActionArgs>(EventType.DialogueCondition, TriggerAction);
        EventManager.Instance.AddListener<AddRemoveCardArgs>(EventType.AddRemoveCard, ChangeCardCondition);
        if (!GameDataManager.Instance.GeneratedChatData.init)
        {
            StartDetectAllParagraph();
        }
        else
        {
            DetectParagraph();
        }
    }
    
    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<SubscribeActionArgs>(EventType.DialogueCondition, TriggerAction);
        EventManager.Instance.RemoveListener<AddRemoveCardArgs>(EventType.AddRemoveCard, ChangeCardCondition);
    }
    
    #region 开始与结束检测

    public void StartDetectAllParagraph()
    {
        //订阅所有段落的触发
        foreach (var paragraph in ReadChatParagraph.Instance.FindAllParagraphData())
        {
            if (paragraph.ParagraphName != "一切的开始")
            {
                ParagraphConditionsToTrigger.Add(paragraph);
            }
        }
        foreach (var paragraphData in ParagraphConditionsToTrigger)
        {
            AddParagraphCondition(paragraphData);
        }
    }

    public void DetectParagraph()
    {
        ParagraphConditionsToTrigger=GameDataManager.Instance.GeneratedChatData.ParagraphConditionsToTrigger;
        foreach (var paragraphData in ParagraphConditionsToTrigger)
        {
            AddParagraphCondition(paragraphData);
        }
    
    }

    public void DetectChatCondition(ChatData chatData)
    {
        //对话判断触发条件，本句有条件时进入，订阅段落触发
        AddChatCondition(chatData);
    }
    
    public void PassParagraphCondition(List<ParagraphData> paragraphData)
    {
        foreach (var data in paragraphData)
        {
            ParagraphConditionsToTrigger.Remove(data);
        }
        //通过对话条件检测时判断该对话是否会打断
        ChatManager.Instance.AddTriggerParagraph(paragraphData[Random.Range(0, paragraphData.Count)]);
    }
    
    
    public void PassChatCondition(List<ChatData> chatDatas)
    {
        foreach (var chatData in chatDatas)
        {
            ChatManager.Instance.CreateMessage(chatData);
        }
    }
    
    
    #endregion
    #region 触发行为
    
    public void TriggerAction(SubscribeActionArgs args)
    {
        Dictionary<string, ParagraphCondition> tmpParagraphDic = new Dictionary<string, ParagraphCondition>(DetectedParagraphConditions);
        foreach (var condition in tmpParagraphDic.Values)
        {
            condition.UpdateProgress(args.type, args.value);
        }
        Dictionary<string, ChatCondition> tmpChatDic = new Dictionary<string, ChatCondition>(DetectedChatConditions);
        foreach (var condition in tmpChatDic.Values)
        {
            condition.UpdateProgress(args.type, args.value);
        }
        
    }
    
    public void ChangeCardCondition(AddRemoveCardArgs args)
    {
        Dictionary<string, ParagraphCondition> tmpParagraphDic = new Dictionary<string, ParagraphCondition>(DetectedParagraphConditions);
        foreach (var condition in tmpParagraphDic.Values)
        {
            condition.UpdateProgress(args.card, args.add);
        }
        Dictionary<string, ChatCondition> tmpChatDic = new Dictionary<string, ChatCondition>(DetectedChatConditions);
        foreach (var condition in tmpChatDic.Values)
        {
            condition.UpdateProgress(args.card, args.add);
        }
    }
    
    #endregion
    #region 检测

    //开始检测
    public void StartChatConditionDetection(GraphData.SerializedNode nodeData)
    {
        AddChatCondition(nodeData.chatData);
    }
    
    public void AddParagraphCondition(ParagraphData paragraphData)
    {
        if (DetectedParagraphConditions.ContainsKey(paragraphData.ParagraphCondition))
        {
            DetectedParagraphConditions[paragraphData.ParagraphCondition].AddData(paragraphData);
        }
        else
        {
            switch (paragraphData.ParagraphCondition)
            {
                case "健康<=0":
                    DetectedParagraphConditions.Add(paragraphData.ParagraphCondition,
                        new HealthZero(paragraphData.ParagraphCondition, true, false,
                            PassParagraphCondition,paragraphData));
                    break;
                case "修理研究完毕":
                    DetectedParagraphConditions.Add(paragraphData.ParagraphCondition,
                        new FinishResearchFix(paragraphData.ParagraphCondition, true, false,
                            PassParagraphCondition,paragraphData));
                    break;
                // case "制作裂缝填充物":
                //     DetectedParagraphConditions.Add(paragraphData.ParagraphCondition,
                //         new MadeCrackFiller(paragraphData.ParagraphCondition, true, false,
                //             PassParagraphCondition,paragraphData));
                //     break;
                case "首次点开气密舱门":
                    DetectedParagraphConditions.Add(paragraphData.ParagraphCondition,
                        new FirstOpenAirtightDoor(paragraphData.ParagraphCondition, true, false,
                            PassParagraphCondition,paragraphData));
                    break;
                case "第一次进入珊瑚礁海域":
                    DetectedParagraphConditions.Add(paragraphData.ParagraphCondition,
                        new FirstEnterCoralIsland(paragraphData.ParagraphCondition, true, false,
                            PassParagraphCondition,paragraphData));
                    break;
                case "每次清醒度<=30":
                    DetectedParagraphConditions.Add(paragraphData.ParagraphCondition,
                        new SobrietyLessThan30(paragraphData.ParagraphCondition, true, false,
                            PassParagraphCondition,paragraphData));
                    break;
                case "第一天5点时未完成修理的研究":
                    DetectedParagraphConditions.Add(paragraphData.ParagraphCondition,
                        new Day1Hour5FixUnConplished(paragraphData.ParagraphCondition, true, false,
                            PassParagraphCondition,paragraphData));
                    break;
                case "第一天11点时未完成修理的研究":
                    DetectedParagraphConditions.Add(paragraphData.ParagraphCondition,
                        new Day1Hour11FixUnConplished(paragraphData.ParagraphCondition, true, false,
                            PassParagraphCondition,paragraphData));
                    break;
                case "第一次堵住渗水裂缝":
                    DetectedParagraphConditions.Add(paragraphData.ParagraphCondition,
                        new SealCracks(paragraphData.ParagraphCondition, true, false,
                            PassParagraphCondition,paragraphData));
                    break;
                case "水平面高度每次达到70":
                    DetectedParagraphConditions.Add(paragraphData.ParagraphCondition,
                        new WaterLevel70(paragraphData.ParagraphCondition, true, false,
                            PassParagraphCondition,paragraphData));
                    break;
                case "水平面高度达到100":
                    DetectedParagraphConditions.Add(paragraphData.ParagraphCondition,
                        new WaterLevel100(paragraphData.ParagraphCondition, true, false,
                            PassParagraphCondition,paragraphData));
                    break;
            }
        }
       
    }

    public void AddChatCondition(ChatData chatData)
    {
        switch (chatData.MessageCondition)
        {
            case "打开摄像头窗口":
                DetectedChatConditions.Add(chatData.MessageCondition,
                    new OpenCameraWindow(chatData.MessageCondition, true, false, PassChatCondition, chatData));
                break;
            case "打开背包窗口":
                DetectedChatConditions.Add(chatData.MessageCondition,
                    new OpenBagWindow(chatData.MessageCondition, true, false, PassChatCondition, chatData));
                break;
            case "打开压缩饼干的详情窗口":
                DetectedChatConditions.Add(chatData.MessageCondition,
                    new OpenDetailBiscuit(chatData.MessageCondition, true, false, PassChatCondition, chatData));
                break;
            case "打开状态窗口":
                DetectedChatConditions.Add(chatData.MessageCondition,
                    new OpenStateWindow(chatData.MessageCondition, true, false, PassChatCondition, chatData));
                break;
            case "打开研究窗口":
                DetectedChatConditions.Add(chatData.MessageCondition,
                    new OpenTechnologyWindow(chatData.MessageCondition, true, false, PassChatCondition, chatData));
                break;
            case "研究修理这项科技":
                if (TechnologyManager.Instance.IsTechNodeComplished("修理") ||
                    TechnologyManager.Instance.IsTechNodeBeingStudied("修理"))
                {
                    PassChatCondition(new List<ChatData>(){chatData});
                }
                DetectedChatConditions.Add(chatData.MessageCondition,
                    new StartResearchFix(chatData.MessageCondition, true, false, PassChatCondition, chatData));
                
                break;
            case "打开地点窗口":
                DetectedChatConditions.Add(chatData.MessageCondition,
                    new OpenLocationWindow(chatData.MessageCondition, true, false, PassChatCondition, chatData));
                break;
            case "点击探索按钮":
                DetectedChatConditions.Add(chatData.MessageCondition,
                    new ClickExploreButton(chatData.MessageCondition, true, false, PassChatCondition, chatData));
                break;
            case "制作裂缝填充物":
                DetectedChatConditions.Add(chatData.MessageCondition,
                    new CreateCrackFiller(chatData.MessageCondition, true, false, PassChatCondition, chatData));
                break;
            case "身上有废金属":
                DetectedChatConditions.Add(chatData.MessageCondition,
                    new HaveMetalInBag(chatData.MessageCondition, true, false, PassChatCondition, chatData));
                break;
        }
    }

    public bool CanTriggerBranchCondition(string name)
    {
        switch (name)
        {
            case "身上有废金属":
                return GameManager.Instance.PlayerBag.FindCardOfName("废金属") != null;
            case "身上没有废金属":
                return GameManager.Instance.PlayerBag.FindCardOfName("废金属") == null;
            case "未装备氧气面罩":
                return GameManager.Instance.EquipmentBag.FindCardOfName("氧气面罩") == null;
            case "已装备氧气面罩":
                return GameManager.Instance.EquipmentBag.FindCardOfName("氧气面罩") != null;
            default:
                return false;
        }
    }

    #endregion
    public void TrackCurrentStatus()
    {
        TimeSpan difference = TimeManager.Instance.CurTime - TimeManager.Instance.StartDateTime;
        if(difference.Days==0&&TimeManager.Instance.CurTime.Hour==5)
        {
            //判断
            if (!TechnologyManager.Instance.IsTechNodeComplished("修理")&&!TechnologyManager.Instance.IsTechNodeBeingStudied("修理"))
            {
                EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("Day1Hour5","FixUnConplished"));
            }
        }
        if(difference.Days==0&&TimeManager.Instance.CurTime.Hour==11)
        {
            //判断
            if (!TechnologyManager.Instance.IsTechNodeComplished("修理")&&!TechnologyManager.Instance.IsTechNodeBeingStudied("修理"))
            {
                EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("Day1Hour11","FixUnConplished"));
            }
        }
    }
}