namespace PRAAPIWEB.Models
{
    public class QuestionDto
    {
        public int Id { get; set; } // Добавляем это

        public string QuestionText { get; set; }
        public List<string> Options { get; set; }
        public string CorrectAnswer { get; set; }
        public string QuestionType { get; set; }
    }



    }
