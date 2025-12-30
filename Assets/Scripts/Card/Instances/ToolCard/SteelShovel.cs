/// <summary>
/// 钢铲
/// </summary>
[CardId("钢铲")]
public class SteelShovel : Card
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