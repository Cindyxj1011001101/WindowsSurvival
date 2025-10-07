public class PopulationConfig
{
    public string CardId; // 卡牌ID
    public int DropNum; // 掉落数量
    public int Size; // 人口数量
    public int MaxSize; // 最大人口数量
    public int SizeChangePerRound; // 每回合数量变化
    public int SizeChangeOnCaught; // 捕捞后的数量变化
    public bool OverwriteFreshness; // 是否覆盖新鲜度
    public bool OverwriteDurability; // 是否覆盖耐久度
    public bool OverwriteGrowth;
    public bool OverwriteProgress; // 是否覆盖产物进度
    public bool Trappable;
}