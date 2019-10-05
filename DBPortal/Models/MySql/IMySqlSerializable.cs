namespace DBPortal.Models.MySql
{
    /// <summary>
    /// Interface for classes which can be represented as an SQL statement.
    /// </summary>
    public interface IMySqlSerializable
    {
        string ToSql();
    }
}