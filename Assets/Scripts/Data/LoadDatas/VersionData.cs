public class VersionData:VersionMigrator
{
    public int Version=GameDataManager.Instance.curVersion;
    public  VersionData()
    {
        Version = GameDataManager.Instance.curVersion;
    }
}

