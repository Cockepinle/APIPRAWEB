namespace PRAAPIWEB.Models
{
    public class LanguageTaskModel
    {
        public int Id { get; set; }
        public int TestId { get; set; } // ID теста, к которому относится задание

        public string Description { get; set; } // Вопрос или инструкция к заданию
        public string Type { get; set; } // Тип задания: "text-choice", "image-choice", "crossword"

        public List<string>? Options { get; set; } // Варианты ответов, если применимо

        public string? ImageUrl { get; set; } // Ссылка на изображение, если тип image-choice
        public string? CorrectAnswer { get; set; } // Правильный ответ (не показываем на клиенте!)
    }
}
