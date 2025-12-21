using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class StudyButton : HoverableButton
{
    private const string TUTORIAL_REQUIRED_TECH_NAME = "修理";

    public Animator gifAnimator;

    private ScriptableTechnologyNode techNode;
    private TechNodeState currentState;

    public void Display(ScriptableTechnologyNode techNode, TechNodeState currentState)
    {
        this.techNode = techNode;
        this.currentState = currentState;

        if (currentState != TechNodeState.BeingStudied) KillAnim();

        if (!GameDataManager.Instance.CurLoad.skipGuide && !TechnologyManager.Instance.IsTechNodeComplished(TUTORIAL_REQUIRED_TECH_NAME))
        {
            // 显示教程
            DisplayTutorial();
            return;
        }

        switch (currentState)
        {
            case TechNodeState.Locked:
                Locked();
                break;
            case TechNodeState.ToStudy:
                ToStudy();
                break;
            case TechNodeState.BeingStudied:
                BeingStudied();
                break;
            case TechNodeState.Complished:
                Complished();
                break;
            case TechNodeState.Queued:
                Queued();
                break;
        }
    }

    #region 新手教程
    private void DisplayTutorial()
    {
        StopBlinking();

        if (techNode.techName == TUTORIAL_REQUIRED_TECH_NAME)
        {
            switch (currentState)
            {
                case TechNodeState.ToStudy:
                    ToStudy();
                    StartBlinking();
                    break;
                case TechNodeState.Queued:
                    Queued();
                    StartBlinking();
                    break;
                case TechNodeState.BeingStudied:
                    BeingStudied();
                    break;
                case TechNodeState.Complished:
                case TechNodeState.Locked:
                    break;
            }
        }
        else
        {
            // 教程需求科技研究完成前不能研究其他科技
            DisplayNotInteractable("暂不可研究", ColorManager.DarkGrey);
        }
    }
    #endregion

    private void DisplayNotInteractable(string tip, Color color)
    {
        gifAnimator.gameObject.SetActive(false);
        Interactable = false;
        text.text = tip;
        text.color = color;
    }

    private void DisplayInteractable(string tip, UnityAction onClick, bool playGif)
    {
        gifAnimator.gameObject.SetActive(true);
        Interactable = true;

        if (playGif)
            PlayAnim();

        this.onClick.RemoveAllListeners();
        this.onClick.AddListener(onClick);

        text.text = tip;
        text.color = ColorManager.White;
    }

    private void Locked()
    {
        DisplayNotInteractable("未解锁", ColorManager.DarkGrey);
    }

    private void Complished()
    {
        DisplayNotInteractable("已完成", ColorManager.Cyan);
    }

    private void BeingStudied()
    {
        DisplayInteractable("研究中", () => TechnologyManager.Instance.StopStudy(), true);
    }

    private void ToStudy()
    {
        if (TechnologyManager.Instance.IsStudyQueueFull)
        {
            DisplayNotInteractable("队列已满", ColorManager.DarkGrey);
            return;
        }

        var tip = TechnologyManager.Instance.IsStudyQueueEmpty ? "开始研究" : "加入队列";
        DisplayInteractable(tip, () => TechnologyManager.Instance.AddToStudyQueue(techNode), false);
    }

    private void Queued()
    {
        var order = TechnologyManager.Instance.GetStudyOrder(techNode);

        var tip = order == 0 ? "继续研究" : "立即研究";

        DisplayInteractable(tip, () => TechnologyManager.Instance.StudyImmediately(techNode), false);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (currentState == TechNodeState.BeingStudied)
        {
            KillAnim();
            text.text = "暂停研究";
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        if (currentState == TechNodeState.BeingStudied)
        {
            PlayAnim();
            text.text = "研究中";
        }
    }

    private void PlayAnim()
    {
        gifAnimator.Play("StudyingGif");
    }

    private void KillAnim()
    {
        if (gifAnimator.gameObject.activeSelf)
            gifAnimator.Play("Default");
    }
}