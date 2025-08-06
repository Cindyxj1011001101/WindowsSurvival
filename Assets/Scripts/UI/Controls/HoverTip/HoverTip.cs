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

    private void Awake()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = canvasGroup.blocksRaycasts = false;
    }

    public void SetTip(
        string textTip,
        int time,
        Dictionary<PlayerStateEnum, float> playerEffects,
        Dictionary<EnvironmentStateEnum, float> envEffects)
    {
        (verticalLayout.transform as RectTransform).sizeDelta = new Vector2((verticalLayout.transform as RectTransform).sizeDelta.x, 1000);

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        // 显示描述
        descText.gameObject.SetActive(!string.IsNullOrEmpty(textTip));

        descText.text = textTip;

        // 显示时间
        timeText.transform.parent.gameObject.SetActive(time > 0);
        timeText.text = $"{time}min";

        // 玩家状态变化
        if (playerEffects != null && playerEffects.Count > 0)
        {
            forPlayer.SetActive(true);

            foreach (var (type, delta) in playerEffects)
            {
                if (StateManager.Instance.PlayerStateDict.TryGetValue(type, out var state))
                {
                    var stateTip = transform.Find($"P_{type}").GetComponent<UIStateTip>();
                    stateTip.SetValue(state, delta);
                    stateTip.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            forPlayer.SetActive(false);
        }

        // 环境状态变化
        if (envEffects != null && envEffects.Count > 0)
        {
            forEnvironment.SetActive(envEffects.Count > 0);

            foreach (var (type, delta) in envEffects)
            {
                if (GameManager.Instance.CurEnvironmentBag.StateDict.TryGetValue(type, out var state))
                {
                    var stateTip = transform.Find($"E_{type}").GetComponent<UIStateTip>();
                    stateTip.SetValue(state, delta);
                    stateTip.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            forEnvironment.SetActive(false);
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
        canvasGroup.DOFade(0, 0.1f).SetEase(Ease.OutQuad);
    }

    public void SelfDestroy()
    {
        canvasGroup.DOKill();
        Destroy(gameObject);
    }
}