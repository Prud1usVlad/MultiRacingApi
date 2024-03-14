namespace MultiRacingApi.Controllers.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<float> Results { get; set; } = new List<float>();
    }
}
