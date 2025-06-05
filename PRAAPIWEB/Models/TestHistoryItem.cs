namespace PRAAPIWEB.Models
{
    public class TestHistoryItem
    {
        public int TestId { get; set; }
        public string TestName { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime CompletedAt { get; set; } // Теперь nullable
    }
}
