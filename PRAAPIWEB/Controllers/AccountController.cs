using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PRAAPIWEB.Models;
using PRAAPIWEB.Services;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace PRAAPIWEB.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;
        private readonly string _apiBaseUrl;

        public AccountController(
            AuthService authService,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<AccountController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _apiBaseUrl = _configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("API base URL is not configured");

            if (!Uri.TryCreate(_apiBaseUrl, UriKind.Absolute, out _))
            {
                throw new InvalidOperationException("Invalid API base URL configuration");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var registerSuccess = await _authService.RegisterAsync(model);
                if (!registerSuccess)
                {
                    ModelState.AddModelError("", "Ошибка регистрации");
                    return View(model);
                }

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при регистрации пользователя");
                ModelState.AddModelError("", $"Ошибка регистрации: {ex.Message}");
                return View(model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var loginResult = await _authService.LoginAsync(model);

                var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, loginResult.User.Email),
    new Claim(ClaimTypes.NameIdentifier, loginResult.User.Id.ToString()), // <-- исправлено
    new Claim(ClaimTypes.GivenName, loginResult.User.Name ?? "Пользователь"),
    new Claim(ClaimTypes.Role, "User")
};

                if (!string.IsNullOrEmpty(loginResult.Token))
                {
                    claims.Add(new Claim("Token", loginResult.Token));
                }


                var claimsIdentity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30) // Увеличиваем срок действия
                    });

                // Устанавливаем куку с токеном, если он есть
                if (!string.IsNullOrEmpty(loginResult.Token))
                {
                    Response.Cookies.Append("access_token", loginResult.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        Expires = DateTimeOffset.UtcNow.AddDays(30),
                        SameSite = SameSiteMode.Lax
                    });
                }

                return RedirectToAction("Index", "AboutProject");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при входе пользователя");
                ModelState.AddModelError("", "Неверный email или пароль");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "AboutProject");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            try
            {
                _logger.LogInformation("Claims: {Claims}", string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));

                var userId = User.FindFirstValue("UserId");
                if (string.IsNullOrEmpty(userId))
                    return RedirectToAction("Login");

                var client = _httpClientFactory.CreateClient("ApiClient");

                // Получаем данные пользователя
                var userResponse = await client.GetAsync($"api/users/{userId}");
                if (!userResponse.IsSuccessStatusCode)
                    return View("Error");

                var user = await userResponse.Content.ReadFromJsonAsync<UserProfile>();

                var token = Request.Cookies["access_token"];
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var testsResponse = await client.GetAsync("api/tests/user/results");
                var testHistory = testsResponse.IsSuccessStatusCode
                    ? await testsResponse.Content.ReadFromJsonAsync<List<TestHistoryItem>>()
                    : new List<TestHistoryItem>();

                

                return View(new ProfileViewModel
                {
                    User = user,
                    TestHistory = testHistory
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки профиля");
                return View("Error");
            }
        }

        private async Task<UserProfile> GetUserData(HttpClient client, string userId)
        {
            try
            {
                var response = await client.GetAsync($"api/users/{userId}");
                if (!response.IsSuccessStatusCode) return null;
                return await response.Content.ReadFromJsonAsync<UserProfile>();
            }
            catch
            {
                return null;
            }
        }

        private async Task<List<TestHistoryItem>> GetTestHistory(HttpClient client)
        {
            try
            {
                var response = await client.GetAsync("api/tests/user/results");
                if (!response.IsSuccessStatusCode) return null;

                var history = await response.Content.ReadFromJsonAsync<List<TestResultDetailDto>>();
                return history?.Select(t => new TestHistoryItem
                {
                    TestId = t.Id,
                    TestName = t.TestName,
                    Score = t.Score,
                    CompletedAt = t.CompletedAt ?? DateTime.MinValue
                }).ToList();
            }
            catch
            {
                return null;
            }
        }

        private async Task<UserProfile> GetUserProfile(HttpClient client, string userId)
        {
            try
            {
                var response = await client.GetAsync($"api/users/{userId}");
                if (!response.IsSuccessStatusCode) return null;
                return await response.Content.ReadFromJsonAsync<UserProfile>();
            }
            catch
            {
                return null;
            }
        }

       
    }
}