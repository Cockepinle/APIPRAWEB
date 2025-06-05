namespace PRAAPIWEB.Models
{
    public class TestDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public List<QuestionViewModel> Questions { get; set; }
        public string TestType { get; set; }
    }
}
