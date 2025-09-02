public interface IVersionMigrator
{
    int ClassVersion { get; set; } 
    IVersionMigrator ToNextVersion();
    IVersionMigrator ToLastestVersion();
    IVersionMigrator ReadJSON(string FilePath,string FileName);
}
public abstract class VersionMigrator : IVersionMigrator
{
    public int ClassVersion { get; set; }

    public virtual IVersionMigrator ToNextVersion()
    {
        return this;
    }

    public virtual IVersionMigrator ToLastestVersion()
    {
        return ToNextVersion();
    }
    
    public virtual IVersionMigrator ReadJSON(string FilePath,string FileName)
    {
        return this;
    }
}