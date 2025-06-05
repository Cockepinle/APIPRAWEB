namespace PRAAPIWEB.Models
{
    public class TestResultResponseDto
    {
        public int Id { get; set; }
        public int TestId { get; set; } // Добавьте это свойство
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}
