namespace PRAAPIWEB.Models
{
    public class LanguageTestModel
    {
        public int Id { get; set; }
        public string Name { get; set; }  // Название теста, например: "Английский для начинающих"
        public string Description { get; set; } // Краткое описание
        public int TotalQuestions { get; set; } // Общее количество заданий
        public TimeSpan? TimeLimit { get; set; } // Ограничение по времени (если есть)
        public string Level { get; set; } // Уровень сложности: A1, A2, B1 и т.п.
        public List<LanguageTaskModel>? Tasks { get; set; } // Задания внутри теста
    }
}
