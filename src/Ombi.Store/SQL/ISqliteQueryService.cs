namespace Ombi.Store.SQL
{
    public interface ISqliteQueryService
    {
        string GetQuery(string resourceId);
    }
}