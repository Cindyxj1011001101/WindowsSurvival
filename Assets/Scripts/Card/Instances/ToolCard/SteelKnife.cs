/// <summary>
/// 钢刀
/// </summary>
[CardId("钢刀")]
public class SteelKnife : Card
{
    protected override void OnLateConstructor()
    {
        base.OnLateConstructor();
        if (TryGetComponent<WeaponComponent>(out var weapon))
        {
            weapon.attackSound = "金属刀攻击声";
        }
    }
}