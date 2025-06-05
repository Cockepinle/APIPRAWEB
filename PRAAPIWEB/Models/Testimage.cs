namespace PRAAPIWEB.Models
{
    public class TestImage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } = "{}"; // Используйте Description вместо Metadata
        public string ImageUrl { get; set; }
        public string TestType { get; set; }
    }
}
