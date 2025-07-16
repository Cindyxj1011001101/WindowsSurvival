using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UITechNode : MonoBehaviour
{
    [SerializeField] private Text techName;
    [SerializeField] private Transform recipeLayout;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Image lockImage;
    [SerializeField] private Image background;

    public void DisplayTechNode(ScriptableTechnologyNode techNode)
    {
        // 显示解锁情况
        lockImage.gameObject.SetActive(TechnologyManager.Instance.IsTechNodeLocked(techNode));

        if (TechnologyManager.Instance.IsTechNodeBeingStudied(techNode))
            background.color = Color.cyan;
        else
            background.color = Color.white;

            // 显示科技名称
            techName.text = techNode.techName;

        // 显示解锁配方
        MonoUtility.DestroyAllChildren(recipeLayout);
        var recipeEntry = Resources.Load<GameObject>("Prefabs/UI/Controls/CardImage");
        foreach (var recipe in techNode.recipes)
        {
            var obj = Instantiate(recipeEntry, recipeLayout);
            obj.transform.Find("Icon").GetComponent<Image>().sprite = recipe.card.cardImage;
        }

        // 显示研究进度和研究速度
        // 研究已完成
        if (TechnologyManager.Instance.IsTechNodeStudied(techNode))
        {
            progressSlider.value = 1;
            progressSlider.GetComponentInChildren<Text>().text = $"已完成";
        }
        // 其他情况
        else
        {
            var progress = TechnologyManager.Instance.GetStudyProgress(techNode);
            progressSlider.value = progress / techNode.cost;
            progressSlider.GetComponentInChildren<Text>().text = $"{progress} / {techNode.cost}";
        }
    }
}