public class StorageBox : Card
{
    private StorageBox()
    {
        Events = new()
        {
            new Event("开启", "开启储物箱", null, null),
            new Event("关闭", "关闭储物箱", null, null)
        };
    }
    
}