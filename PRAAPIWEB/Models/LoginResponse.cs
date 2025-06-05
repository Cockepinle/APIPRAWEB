namespace PRAAPIWEB.Models
{
    public class LoginResponse
    {
        public string Token { get; set; }  // Добавьте это поле
        public string Message { get; set; }
        public UserModel User { get; set; }
    }
}