namespace PRAAPIWEB.Models
{
    public class TestResultViewModel
    {
        public int TestId { get; set; }
        public string TestName { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public List<QuestionResultViewModel> Questions { get; set; }
        public List<QuestionResultViewModel> QuestionResults { get; set; } // Для совместимости

    }
}
