namespace PRAAPIWEB.Models
{
    public class ProfileViewModel
    {
        public UserProfile User { get; set; }
        public List<TestHistoryItem> TestHistory { get; set; } = new List<TestHistoryItem>();
    }
}
