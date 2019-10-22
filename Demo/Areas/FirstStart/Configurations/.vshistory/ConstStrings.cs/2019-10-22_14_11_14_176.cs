namespace Demo.Areas.FirstStart.Configurations
{
    public static class ConstStrings
    {

        public static string AdminUserName = nameof(AdminUserName);
        public static string AdminEmail = nameof(AdminEmail);
        public static string AdminPassword = nameof(AdminPassword);

        public static string FirstStart = $"/{nameof(FirstStart)}";
        public static string IsFirstStart = nameof(IsFirstStart);
        public static string Administrators = nameof(Administrators);

        public static string DataProvider = nameof(DataProvider);
        public static string SqlServerConnection = nameof(SqlServerConnection);
        public static string InMemoryConnection = nameof(InMemoryConnection);
        public static string PostgreSQLConnection = nameof(PostgreSQLConnection);
        public static string SQLiteConnection = nameof(SQLiteConnection);
    }
}
