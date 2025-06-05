namespace PRAAPIWEB.Models
{
    public class TestResultSubmissionDto
    {
        public int TestId { get; set; }
        public int Score { get; set; }
        public List<QuestionAnswerDto> Answers { get; set; }
    }
}
