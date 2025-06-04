namespace AIRecipeApp.Api.Models
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public List<string> Roles { get; set; }
    }
}