using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PRAAPIWEB.Models
{
    public class UserAnswer
    {
        public int QuestionId { get; set; }  // Это поле обязательно!
        public string Answer { get; set; }
        [BindNever]
        public bool IsCorrect { get; set; } // Добавьте это свойство


    }

}
