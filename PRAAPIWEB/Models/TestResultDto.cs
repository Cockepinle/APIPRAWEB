namespace PRAAPIWEB.Models {
    public class TestResultDto
    {
        public int TestId { get; set; }
        public List<TestAnswerDto> Answers { get; set; } = new();
    }
}

