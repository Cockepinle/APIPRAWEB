namespace PRAAPIWEB.Models
{
    public class QuestionResultViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string CorrectAnswer { get; set; }
        public string UserAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public string Explanation { get; set; } // Добавляем недостающее свойство

    }
}
