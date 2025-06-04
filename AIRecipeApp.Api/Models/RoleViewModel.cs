namespace AIRecipeApp.Api.Models
{
    public class RoleViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Permissions { get; set; }
    }
}