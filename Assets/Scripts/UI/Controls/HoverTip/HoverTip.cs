using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoverTip : MonoBehaviour
{
    public Text descText;
    public Text timeText;
    public GameObject forPlayer;
    public GameObject forEnvironment;

    public VerticalLayoutGroup verticalLayout;
    public CanvasGroup canvasGroup;

    private float maxWidth;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        canvasGroup.alpha = 0;
        canvasGroup.interactable = canvasGroup.blocksRaycasts = false;
        maxWidth = rectTransform.sizeDelta.x;
    }

    private void OnDisable()
    {
        canvasGroup.DOKill();
        canvasGroup.alpha = 0;
    }

    public void SetTip(
        string textTip,
        Color textColor,
        int time,
        Dictionary<PlayerStateEnum, float> playerStateChanges,
        Dictionary<EnvironmentStateEnum, float> envStateChanges)
    {
        descText.color = textColor;

        bool textTipOnly = true;

        (verticalLayout.transform as RectTransform).sizeDelta = new Vector2((verticalLayout.transform as RectTransform).sizeDelta.x, 1000);

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        // 显示描述
        if (string.IsNullOrEmpty(textTip))
        {
            textTipOnly = false;
            descText.gameObject.SetActive(false);
        }
        else
        {
            descText.gameObject.SetActive(true);
            descText.text = textTip;
        }

        // 显示时间
        if (time > 0)
        {
            textTipOnly = false;
            timeText.transform.parent.gameObject.SetActive(true);
            timeText.text = $"{time}min";
        }
        else
        {
            timeText.transform.parent.gameObject.SetActive(false);
        }

        // 玩家状态变化
        if (playerStateChanges.IsNullOrEmpty())
        {
            forPlayer.SetActive(false);
        }
        else
        {
            textTipOnly = false;

            forPlayer.SetActive(true);

            foreach (var (type, delta) in playerStateChanges)
            {
                if (StateManager.Instance.PlayerStateDict.TryGetValue(type, out var state))
                {
                    var stateTip = transform.Find($"P_{type}").GetComponent<UIStateTip>();
                    stateTip.SetValue(state, delta);
                    stateTip.gameObject.SetActive(true);
                }
            }
        }

        // 环境状态变化
        if (envStateChanges.IsNullOrEmpty())
        {
            forEnvironment.SetActive(false);
        }
        else
        {
            textTipOnly = false;

            forEnvironment.SetActive(envStateChanges.Count > 0);

            foreach (var (type, delta) in envStateChanges)
            {
                if (type == EnvironmentStateEnum.Electricity)
                {
                    var stateTip = transform.Find($"E_{type}").GetComponent<UIStateTip>();
                    stateTip.SetValue(ElectricPowerManager.Instance.Power, delta);
                    stateTip.gameObject.SetActive(true);
                    continue;
                }

                if (type == EnvironmentStateEnum.WaterLevel)
                {
                    var stateTip = transform.Find($"E_{type}").GetComponent<UIStateTip>();
                    stateTip.SetValue(StateManager.Instance.WaterLevel, delta);
                    stateTip.gameObject.SetActive(true);
                    continue;
                }

                if (GameManager.Instance.CurEnvironmentBag.StateDict.TryGetValue(type, out var state))
                {
                    var stateTip = transform.Find($"E_{type}").GetComponent<UIStateTip>();
                    stateTip.SetValue(state, delta);
                    stateTip.gameObject.SetActive(true);
                }
            }
        }


        // 如果仅显示文本
        if (textTipOnly)
        {
            // 自适应长度
            rectTransform.sizeDelta = new Vector2(Mathf.Min(maxWidth, descText.preferredWidth + verticalLayout.padding.left + verticalLayout.padding.right), rectTransform.sizeDelta.y);
        }
        else
        {
            rectTransform.sizeDelta = new Vector2(maxWidth, rectTransform.sizeDelta.y);
        }

        // 更新高度
        MonoUtility.UpdateVerticalLayoutSize(verticalLayout);
    }

    public void Show()
    {
        canvasGroup.DOKill();
        canvasGroup.DOFade(1, 0.1f).SetEase(Ease.OutQuad);
    }

    public void Hide()
    {
        canvasGroup.DOKill();
        canvasGroup.DOFade(0, 0.1f).SetEase(Ease.OutQuad).OnComplete(() => ObjectBufferPool.Instance.Restore(gameObject));
    }
}