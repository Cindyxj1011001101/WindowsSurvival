using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ChatConditionManager : MonoBehaviour
{
    public static ChatConditionManager Instance { get; private set; }

    public Dictionary<string, Condition> DetectedConditions = new Dictionary<string, Condition>();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EventManager.Instance.AddListener<SubscribeActionArgs>(EventType.DialogueCondition, TriggerAction);
        DetectParagraph();
    }

    private void DetectParagraph()
    {
        //订阅所有段落的触发
        foreach (var paragraph in ChatManager.Instance.ParagraphDataList)
        {
            AddParagraphCondition(paragraph);
            //Debug.Log($"开始检测段落条件: {paragraph.TriggerParagraphCondition}");
        }
    }
    public void DetectChatCondition(ChatData chatData)
    {
        //对话判断触发条件，本句有条件时进入，订阅段落触发
        AddChatCondition(chatData);
    }
    public void PassParagraphCondition(ParagraphData paragraphData)
    {
        ChatManager.Instance.TriggerParagraph(paragraphData);
    }


    public void PassChatCondition(ChatData chatData)
    {
        ChatManager.Instance.NextMessage = chatData;
    }

    #region 触发行为
    public void TriggerAction(SubscribeActionArgs args)
    {
        //Debug.Log($"触发行为: {args.type} {args.value}");
        Dictionary<string, Condition> tmpDic=new Dictionary<string, Condition>(DetectedConditions);
        foreach (var condition in tmpDic.Values)
        {
            condition.UpdateProgress(args.type, args.value);
        }
    }
    #endregion

    #region 检测
    //开始检测
    public void StartChatConditionDetection(ChatData chatData)
    {
        AddChatCondition(chatData);
        //Debug.Log($"开始检测对话条件: {chatData.MessageCondition}");
    }
    public void AddParagraphCondition(ParagraphData paragraphData)
    {
        switch (paragraphData.TriggerParagraphCondition)
        {
            case "健康<=0":
                DetectedConditions.Add(paragraphData.TriggerParagraphCondition,
                new HealthZero(paragraphData.TriggerParagraphCondition, true, false, () => PassParagraphCondition(paragraphData)));
                break;
            case "“修理”研究完毕":
                DetectedConditions.Add(paragraphData.TriggerParagraphCondition,
                new FinishResearchFix(paragraphData.TriggerParagraphCondition, true, false, () => PassParagraphCondition(paragraphData)));
                break;
        }
    }

    public void AddChatCondition(ChatData chatData)
    {
        switch (chatData.MessageCondition)
        {
            case "打开摄像头窗口":
                DetectedConditions.Add(chatData.MessageCondition,
                new OpenCameraWindow(chatData.MessageCondition, true, false, () => PassChatCondition(chatData)));
                break;
            case "打开背包窗口":
                DetectedConditions.Add(chatData.MessageCondition,
                new OpenBagWindow(chatData.MessageCondition, true, false, () => PassChatCondition(chatData)));
                break;
            case "打开“压缩饼干”的详情窗口":
                DetectedConditions.Add(chatData.MessageCondition,
                new OpenDetailBiscuit(chatData.MessageCondition, true, false, () => PassChatCondition(chatData)));
                break;
            case "打开“状态窗口”":
                DetectedConditions.Add(chatData.MessageCondition,
                new OpenStateWindow(chatData.MessageCondition, true, false, () => PassChatCondition(chatData)));
                break;
            case "打开“研究窗口”":
                DetectedConditions.Add(chatData.MessageCondition,
                new OpenTechnologyWindow(chatData.MessageCondition, true, false, () => PassChatCondition(chatData)));
                break;
            case "研究“修理”这项科技":
                DetectedConditions.Add(chatData.MessageCondition,
                new StartResearchFix(chatData.MessageCondition, true, false, () => PassChatCondition(chatData)));
                break;
            case "打开地点窗口":
                DetectedConditions.Add(chatData.MessageCondition,
                new OpenLocationWindow(chatData.MessageCondition, true, false, () => PassChatCondition(chatData)));
                break;
            case "点击探索按钮":
                DetectedConditions.Add(chatData.MessageCondition,
                new ClickExploreButton(chatData.MessageCondition, true, false, () => PassChatCondition(chatData)));
                break;

            case "制作“裂缝填充物”":
                DetectedConditions.Add(chatData.MessageCondition,
                new CreateCrackFiller(chatData.MessageCondition, true, false, () => PassChatCondition(chatData)));
                break;
            case "身上有“废金属”":
                DetectedConditions.Add(chatData.MessageCondition,
                new HaveMetalInBag(chatData.MessageCondition, true, false, () => PassChatCondition(chatData)));
                break;

        }
    }
    #endregion
}