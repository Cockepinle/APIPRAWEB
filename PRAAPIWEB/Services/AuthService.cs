using Newtonsoft.Json;
using PRAAPIWEB.Models;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace PRAAPIWEB.Services
{
    public class AuthService
    {
        private readonly HttpClient _client;

        public AuthService(HttpClient client) // ← Должен быть public!
        {
            _client = client;
            _client.BaseAddress = new Uri("https://apipra-production.up.railway.app/");
        }

        public async Task<LoginResponse> LoginAsync(LoginModel model)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("api/Account/login", model);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Ошибка входа: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Получен ответ: {json}"); // Логируем ответ

                var responseObj = JObject.Parse(json);

                // Проверяем наличие обязательных полей
                if (responseObj["user"] == null)
                {
                    throw new Exception("Ответ сервера не содержит данных пользователя");
                }

                return new LoginResponse
                {
                    Message = responseObj["message"]?.ToString(),
                    User = new UserModel
                    {
                        Id = (int)responseObj["user"]["id"],
                        Email = responseObj["user"]["email"]?.ToString(),
                        Name = responseObj["user"]["name"]?.ToString() ?? "Пользователь"
                    },
                    Token = responseObj["token"]?.ToString() // Добавляем токен, если есть
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в LoginAsync: {ex}");
                throw new Exception("Ошибка при входе в систему", ex);
            }
        }

        public async Task<bool> RegisterAsync(RegisterRequest model)
        {
            try
            {
                Console.WriteLine($"Register attempt: {model.Email}");
                var response = await _client.PostAsJsonAsync("api/Account/register", model);

                Console.WriteLine($"Register status: {response.StatusCode}");
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Register error: {content}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Register exception: {ex}");
                return false;
            }
        }
        public async Task CreateUserAsync(User user)
        {
            var response = await _client.PostAsJsonAsync("api/Account/createuser", user);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"CreateUser failed: {error}");
            }
        }
        public async Task<UserModel> GetUserProfileAsync(int userId)
        {
            try
            {
                var response = await _client.GetAsync($"api/Account/user/{userId}");

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Ошибка при получении данных профиля");

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<UserModel>(json);

                return result ?? throw new Exception("Неверный формат ответа");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в GetUserProfileAsync: {ex}");
                throw;
            }
        }

        public async Task<List<UserTestResult>> GetUserTestResultsAsync(int userId)
        {
            try
            {
                var response = await _client.GetAsync($"api/Account/test-results/{userId}");

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Ошибка при получении результатов тестов");

                var json = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<List<UserTestResult>>(json);

                return results ?? new List<UserTestResult>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в GetUserTestResultsAsync: {ex}");
                return new List<UserTestResult>();
            }
        }
    }
}
