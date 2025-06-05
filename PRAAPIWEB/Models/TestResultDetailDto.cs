namespace PRAAPIWEB.Models
{
    public class TestResultDetailDto
    {
        public int Id { get; set; }
        public string TestName { get; set; }
        public int Score { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
