namespace DBPortal.Models.MySql
{
    /// <summary>
    /// Model class for a MySQL user.
    /// </summary>
    public class User : IMySqlSerializable
    {
        /// <summary>
        /// The default user account.
        /// </summary>
        public static User Default => new User
        {
            Username = "admin",
            Password = "password"
        };
        
        public string Username;

        public string Password;

        public string ToSql()
        {
            return $"CREATE USER '{Username}'@'%' IDENTIFIED BY '{Password}';\n"
                   + $"GRANT ALL ON *.* TO '{Username}'@'%' WITH GRANT OPTION;\n";
        }
    }
}