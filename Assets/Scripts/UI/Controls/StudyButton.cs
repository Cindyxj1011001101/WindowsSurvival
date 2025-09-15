using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StudyButton : HoverableButton
{
    public Text text;
    public GameObject iconObject;

    private bool beingStudied = false;

    public Animator studyingAnim;
    
    public void DisplayButton(ScriptableTechnologyNode techNode, UnityAction startStuyding, UnityAction stopStudying)
    {
        #region 新手教程
        if (!GameDataManager.Instance.CurLoad.SkipGuide) // 如果新手教程未跳过
        {
            if (techNode.techName == "修理")
            {
                // “修理”科技未完成，按钮闪烁提示
                if (!TechnologyManager.Instance.IsTechNodeComplished("修理") &&
                !TechnologyManager.Instance.IsTechNodeBeingStudied(techNode))
                    StartBlinking();
                else
                    StopBlinking();
            }
            else
            {
                StopBlinking();
                // “修理”科技研究完成前不能研究其他科技
                if (!TechnologyManager.Instance.IsTechNodeComplished("修理") && techNode.techName != "修理")
                {
                    iconObject.SetActive(false);
                    Interactable = false;
                    text.text = "暂不可研究";
                    text.color = ColorManager.DarkGrey;
                    return;
                }
            }
        }
        #endregion

        beingStudied = TechnologyManager.Instance.IsTechNodeBeingStudied(techNode);

        if (!beingStudied)
            KillAnim();

        // 研究已完成
        if (TechnologyManager.Instance.IsTechNodeComplished(techNode))
        {
            iconObject.SetActive(false);
            Interactable = false;
            text.text = "已完成";
            text.color = ColorManager.Cyan;
        }
        // 研究正在进行
        else if (beingStudied)
        {
            iconObject.SetActive(true);
            Interactable = true;
            // 播放动效
            PlayAnim();

            // 点击暂停研究
            onClick.RemoveAllListeners();
            onClick.AddListener(stopStudying);

            text.text = "研究中";
            text.color = ColorManager.White;
        }
        // 研究未解锁
        else if (TechnologyManager.Instance.IsTechNodeLocked(techNode, out _))
        {
            iconObject.SetActive(false);
            Interactable = false;
            text.text = "未解锁";
            text.color = ColorManager.DarkGrey;
        }
        // 可以进行研究
        else
        {
            iconObject.SetActive(true);
            Interactable = true;

            // 点击开始研究
            onClick.RemoveAllListeners();
            onClick.AddListener(startStuyding);

            text.text = "开始研究";
            text.color = ColorManager.White;
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (beingStudied)
        {
            KillAnim();
            text.text = "暂停研究";
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        if (beingStudied)
        {
            text.text = "研究中";
            PlayAnim();
        }
    }

    private void PlayAnim()
    {
        studyingAnim.ResetTrigger("Stop");
        studyingAnim.SetTrigger("Play");
    }

    private void KillAnim()
    {
        studyingAnim.ResetTrigger("Play");
        studyingAnim.SetTrigger("Stop");
    }
}