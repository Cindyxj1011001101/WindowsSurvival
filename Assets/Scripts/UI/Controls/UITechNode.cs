using UnityEngine;
using UnityEngine.UI;

public class UITechNode : MonoBehaviour
{
    [SerializeField] private Text techName;
    [SerializeField] private Transform recipeLayout;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Image lockImage;

    public void DisplayTechNode(ScriptableTechnologyNode techNode)
    {
        // 显示解锁情况
        lockImage.gameObject.SetActive(TechnologyManager.Instance.IsTechNodeLocked(techNode));

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
            progressSlider.GetComponentInChildren<Text>().text = $"{techNode.cost} / {techNode.cost}";
        }
        // 研究正在进行
        else if (TechnologyManager.Instance.IsTechNodeBeingStudied(techNode))
        {
            progressSlider.value = TechnologyManager.Instance.CurProgress / techNode.cost;
            progressSlider.GetComponentInChildren<Text>().text = $"{TechnologyManager.Instance.CurProgress} / {techNode.cost}";
        }
        // 其他不能研究的情况
        else
        {
            progressSlider.value = 0;
            progressSlider.GetComponentInChildren<Text>().text = $"0 / {techNode.cost}";
        }
    }
}