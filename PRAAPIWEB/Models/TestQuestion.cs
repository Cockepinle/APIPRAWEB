namespace PRAAPIWEB.Models
{
    public class TestQuestion
    {
        public int Id { get; set; }

        public string Question { get; set; }
        public string QuestionType { get; set; }
        public string Answer { get; set; }
        public List<string> Options { get; set; } = new List<string>(); // Добавляем Options
    }

}
