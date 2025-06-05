namespace PRAAPIWEB.Models
{
    public class QuestionResult
    {
        public string QuestionText { get; set; }
        public string CorrectAnswer { get; set; }
        public string UserAnswer { get; set; }
        public bool IsCorrect => UserAnswer == CorrectAnswer;
    }
}
