using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Linq;

public class ProcessManager
{
	public static ProcessManager Instance { get; } = new ProcessManager();

	private List<ProcessConfig> processConfigList;

	private ProcessManager() { }

	public void Init()
	{
		processConfigList = ExcelReader.ReadProcessConfig("ProcessConfig");
	}

	public string GetProcessOutcomeID(List<Card> processedCards, Dictionary<TemperatureType, int> temperatureRecord)
	{
		return PickFromMatchedProcess(FindMatchedProcess(processedCards, temperatureRecord)).OutcomeID;
	}

	/// <summary>
	/// 根据传入卡牌与各温度回合数判断可加工配方
	/// </summary>
	/// <param name="processedCards"></param>
	/// <param name="temperatureRecord"></param>
	/// <returns></returns>
	private List<ProcessConfig> FindMatchedProcess(List<Card> processedCards, Dictionary<TemperatureType, int> temperatureRecord)
	{
		List<ProcessConfig> result = new();
		foreach (ProcessConfig config in processConfigList)
		{
			if (!IsFoodPropertyRequirementMet(config.FoodPropertyRequirementList, processedCards))
				continue;

			if (!IsCardRequirementMet(config.CardRequirementList, processedCards))
				continue;

			if (!IsTempertureRequirementMet(config, temperatureRecord))
				continue;

			result.Add(config);
		}
		return result;
	}

	/// <summary>
	/// 从满足的配方中选择一个
	/// </summary>
	/// <param name="matchedConfigs"></param>
	/// <returns></returns>
	private ProcessConfig PickFromMatchedProcess(List<ProcessConfig> matchedConfigs)
	{
		// 按照优先级降序排序
		matchedConfigs.Sort((x, y) => y.Priority.CompareTo(x.Priority));

		int maxPriority = matchedConfigs[0].Priority;

		List<ProcessConfig> result = matchedConfigs.Where(config => config.Priority == maxPriority).ToList();

		// 从优先级最高的配方中随机选一个
		return result[Random.Range(0, result.Count)];
	}

	/// <summary>
	/// 判断食物属性是否符合条件
	/// </summary>
	/// <param name="requirements"></param>
	/// <param name="processedCards"></param>
	/// <returns></returns>
	private bool IsFoodPropertyRequirementMet(List<FoodPropertyRequirement> requirements, List<Card> processedCards)
	{
		foreach (var re in requirements)
		{
			// 统计所有卡牌的该食物属性值
			int foodPropertyValue = processedCards.Sum(c =>
			{
				c.TryGetComponent<FoodPropertyComponent>(out var comp);
				return comp.foodPropertyDict[re.foodProperty];
			});

			// 任意一项不满足则返回false
			if (!re.IsMet(foodPropertyValue))
				return false;
		}

		return true;
	}

	/// <summary>
	/// 判断卡牌数量是否符合条件
	/// </summary>
	/// <param name="requirements"></param>
	/// <param name="processedCards"></param>
	/// <returns></returns>
	private bool IsCardRequirementMet(List<CardRequirement> requirements, List<Card> processedCards)
	{
		foreach (var re in requirements)
		{
			// 统计卡牌数量
			int cardCount = processedCards.Count(c => re.requiredCardIdList.Contains(c.CardId));

			// 任意一项不满足则返回false
			if (!re.IsMet(cardCount))
				return false;
		}

		return true;
	}

	/// <summary>
	/// 判断温度满足的回合数是否符合条件
	/// </summary>
	/// <param name="config"></param>
	/// <param name="temperatureRecord"></param>
	/// <returns></returns>
	private bool IsTempertureRequirementMet(ProcessConfig config, Dictionary<TemperatureType, int> temperatureRecord)
	{
		int roundCount = 0;
		foreach ((TemperatureType temperature, int round) in temperatureRecord)
		{
			if (config.TempertureRequirementList.Contains(temperature))
			{
				roundCount += round;
			}
		}
		return config.RoundRequirement.IsMet(roundCount);
	}
}
