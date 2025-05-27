namespace AIRecipeApp.Api.Authorization
{
    public static class Policies
    {
        public const string AdminOnly = "AdminOnly";
        public const string ModeratorOrAdmin = "ModeratorOrAdmin";
        public const string UserOrAbove = "UserOrAbove";
    }

    public static class Claims
    {
        public const string Role = "role";
        public const string UserId = "user_id";
        public const string Username = "username";
    }
} 