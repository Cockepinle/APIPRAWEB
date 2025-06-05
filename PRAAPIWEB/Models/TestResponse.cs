namespace PRAAPIWEB.Models
{
    public class TestResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } // Просто описание теста
        public string ImageUrl { get; set; }
        public List<TestQuestion> Questions { get; set; } // Вопросы как отдельное поле
        public string TestType { get; set; }
    }

}
