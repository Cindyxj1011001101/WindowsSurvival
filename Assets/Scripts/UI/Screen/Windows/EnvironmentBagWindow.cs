using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum PressureType
{
    极低,
    低,
    标准,
    高,
    极高
}

public class EnvironmentBagWindow : BagWindow
{
    private Text discoveryDegreeText; // 探索度显示
    private Text placeNameText; // 环境名称
    private Text placeDetailsText; // 环境详情
    private Button discoverButton; // 探索按钮
    private Dictionary<EnvironmentStateEnum, GameObject> EnvironmentStateSliders = new Dictionary<EnvironmentStateEnum, GameObject>(); // 环境状态显示

    protected override void Awake()
    {
        base.Awake();

        discoveryDegreeText = transform.Find("LeftBar/DiscoveryDegree").GetComponent<Text>();
        placeNameText = transform.Find("TopBar/Name").GetComponent<Text>();
        placeDetailsText = transform.Find("LeftBar/Details/Text").GetComponent<Text>();
        discoverButton = transform.Find("LeftBar/DiscoverButton").GetComponent<Button>();
        EnvironmentStateSliders.Add(EnvironmentStateEnum.Electricity, transform.Find("LeftBar/EnvironmentState/BagScrollView/Viewport/Container/Electricity").gameObject);
        EnvironmentStateSliders.Add(EnvironmentStateEnum.Oxygen, transform.Find("LeftBar/EnvironmentState/BagScrollView/Viewport/Container/Oxygen").gameObject);
        EnvironmentStateSliders.Add(EnvironmentStateEnum.Pressure, transform.Find("LeftBar/EnvironmentState/BagScrollView/Viewport/Container/Pressure").gameObject);
        EnvironmentStateSliders.Add(EnvironmentStateEnum.Height, transform.Find("LeftBar/EnvironmentState/BagScrollView/Viewport/Container/Height").gameObject);

        // 注册发现度变化事件
        EventManager.Instance.AddListener<float>(EventType.ChangeDiscoveryDegree, DisplayDiscoveryDegree);
        // 注册环境移动事件
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.Move, OnMove);
        // 注册环境状态变化事件
        EventManager.Instance.AddListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnEnvironmentChangeState);
    }

    protected override void Init()
    {
        GameManager.Instance.Move(GameManager.Instance.CurEnvironmentBag.PlaceData.placeType);
    }

    /// <summary>
    /// 移动到指定环境袋
    /// </summary>
    private void OnMove(EnvironmentBag curEnvironmentBag)
    {
        // 新的环境信息
        // TODO：所有环境状态变化UI刷新,当前环境中不包含的UI关闭
        foreach (var state in EnvironmentStateSliders)
        {
            //室内显示氧气
            if (curEnvironmentBag.PlaceData.isIndoor)
            {
                EnvironmentStateSliders[EnvironmentStateEnum.Oxygen].SetActive(true);
            }
            else
            {
                EnvironmentStateSliders[EnvironmentStateEnum.Oxygen].SetActive(false);
            }
            //飞船中显示水平面高度
            if (curEnvironmentBag.PlaceData.isInSpacecraft)
            {
                EnvironmentStateSliders[EnvironmentStateEnum.Height].SetActive(true);
            }
            else
            {
                EnvironmentStateSliders[EnvironmentStateEnum.Height].SetActive(false);
            }
        }
        discoveryDegreeText.text = $"{Math.Round(curEnvironmentBag.DiscoveryDegree, 3) * 100} %";
        placeNameText.text = $"{curEnvironmentBag.PlaceData.placeName}";
        placeDetailsText.text = $"{curEnvironmentBag.PlaceData.placeDesc}";
        // 探索事件
        discoverButton.onClick.RemoveAllListeners();
        discoverButton.onClick.AddListener(GameManager.Instance.HandleExplore);
    }
    /// <summary>
    /// 单个环境状态变化UI刷新
    /// </summary>
    private void OnEnvironmentChangeState(RefreshEnvironmentStateArgs args)
    {
        if (args.place == GameManager.Instance.CurEnvironmentBag.PlaceData.placeType)
        {
            if (EnvironmentStateSliders.ContainsKey(args.state) && GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict.ContainsKey(args.state))
            {
                float resultValue = 0;
                //TODO：压强显示为文字
                if (args.state == EnvironmentStateEnum.Electricity)
                {
                    resultValue = StateManager.Instance.Electricity / GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict[args.state].MaxValue;
                    EnvironmentStateSliders[args.state].GetComponentInChildren<Slider>().value = resultValue;
                    EnvironmentStateSliders[args.state].transform.Find("StateNum").GetComponent<TMP_Text>().text =
                    StateManager.Instance.Electricity.ToString("f1") + "/"
                    + GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict[args.state].MaxValue.ToString();
                }
                else if (args.state == EnvironmentStateEnum.Pressure)
                {
                    resultValue = GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict[args.state].curValue
                    / GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict[args.state].MaxValue;
                    EnvironmentStateSliders[args.state].GetComponentInChildren<Slider>().value = resultValue;
                    string showText = "";
                    switch (GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict[args.state].curValue)
                    {
                        case 0:
                            showText = "极低";
                            break;
                        case 1:
                            showText = "低";
                            break;
                        case 2:
                            showText = "标准";
                            break;
                        case 3:
                            showText = "高";
                            break;
                        case 4:
                            showText = "极高";
                            break;
                    }
                    EnvironmentStateSliders[args.state].transform.Find("StateNum").GetComponent<TMP_Text>().text = showText;
                }
                else
                {
                    resultValue = GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict[args.state].curValue
                    / GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict[args.state].MaxValue;
                    EnvironmentStateSliders[args.state].GetComponentInChildren<Slider>().value = resultValue;
                    EnvironmentStateSliders[args.state].transform.Find("StateNum").GetComponent<TMP_Text>().text =
                    GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict[args.state].curValue.ToString("f1") + "/"
                    + GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict[args.state].MaxValue.ToString();
                }
            }
        }
    }

    private void DisplayDiscoveryDegree(float degree)
    {
        discoveryDegreeText.text = $"{Math.Round(degree, 3) * 100} %";
    }

    private void OnDestroy()
    {
        // 移除事件
        EventManager.Instance.RemoveListener<float>(EventType.ChangeDiscoveryDegree, DisplayDiscoveryDegree);
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.Move, OnMove);
        EventManager.Instance.RemoveListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnEnvironmentChangeState);
    }
}
