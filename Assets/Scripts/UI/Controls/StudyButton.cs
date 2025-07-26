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
        beingStudied = TechnologyManager.Instance.IsTechNodeBeingStudied(techNode);

        if (!beingStudied)
            KillAnim();

        // 研究已完成
        if (TechnologyManager.Instance.IsTechNodeComplished(techNode))
        {
            iconObject.SetActive(false);
            Interactable = false;
            text.text = "已完成";
            text.color = ColorManager.cyan;
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
            text.color = ColorManager.white;
        }
        // 研究未解锁
        else if (TechnologyManager.Instance.IsTechNodeLocked(techNode))
        {
            iconObject.SetActive(false);
            Interactable = false;
            text.text = "未解锁";
            text.color = ColorManager.darkGrey;
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
            text.color = ColorManager.white;
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