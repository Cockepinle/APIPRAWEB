using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using PRAAPIWEB.Models;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace PRAAPIWEB.Controllers
{
    [Authorize]
    public class TestsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;

        public TestsController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AccountController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // Список всех тестов
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                client.Timeout = TimeSpan.FromSeconds(10); // установите 

                var response = await client.GetAsync("api/tests");

                if (!response.IsSuccessStatusCode)
                {
                    // Логировать ошибку
                    return View("Error", new ErrorViewModel
                    {
                        ErrorMessage = $"Ошибка сервера: {response.StatusCode}"
                    });
                }

                var tests = await response.Content.ReadFromJsonAsync<List<TestDto>>(); // измените на TestDto
                return View(tests);
            }
            catch (TaskCanceledException)
            {
                return View("Error", new ErrorViewModel
                {
                    ErrorMessage = "Превышено время ожидания ответа от сервера"
                });
            }
            catch (Exception ex)
            {
                // Логировать ошибку
                return View("Error", new ErrorViewModel
                {
                    ErrorMessage = $"Неожиданная ошибка: {ex.Message}"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Start(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                var response = await client.GetAsync($"api/tests/{id}");

                if (!response.IsSuccessStatusCode)
                    return View("Error", new ErrorViewModel { ErrorMessage = "Не удалось загрузить тест." });

                var testData = await response.Content.ReadFromJsonAsync<TestResponse>();
                if (testData == null)
                    return View("Error", new ErrorViewModel { ErrorMessage = "Тест не найден." });

                // Если Options не приходят с API, создаем их на основе ответа
                foreach (var question in testData.Questions)
                {
                    if (question.Options == null || !question.Options.Any())
                    {
                        question.Options = new List<string> { question.Answer, "Другой вариант 1", "Другой вариант 2" };
                    }
                }

                var testDetail = new TestDetailDto
                {
                    Id = testData.Id,
                    Name = testData.Name,
                    ImageUrl = testData.ImageUrl,
                    Questions = testData.Questions.Select(q => new QuestionDto
                    {
                        QuestionText = q.Question,
                        Options = q.Options,
                        CorrectAnswer = q.Answer,
                        QuestionType = q.QuestionType
                    }).ToList()
                };

                return View(testDetail);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { ErrorMessage = $"Ошибка: {ex.Message}" });
            }
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int testId, List<UserAnswer> answers)
        {
            try
            {
                _logger.LogInformation("Ответы пользователя: {@Answers}", answers);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


                var client = _httpClientFactory.CreateClient("ApiClient");
                var token = await HttpContext.GetTokenAsync("access_token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Определяем правильность ответов перед отправкой
                var questionsResponse = await client.GetAsync($"api/tests/{testId}/questions");
                if (!questionsResponse.IsSuccessStatusCode)
                {
                    return RedirectToAction("TestError", new { testId });
                }

                var questions = await questionsResponse.Content.ReadFromJsonAsync<List<TestQuestion>>();

                // Помечаем правильность ответов
                foreach (var answer in answers)
                {
                    var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);

                    if (question == null || string.IsNullOrWhiteSpace(question.Answer) || string.IsNullOrWhiteSpace(answer.Answer))
                    {
                        answer.IsCorrect = false;
                    }
                    else
                    {
                        answer.IsCorrect = question.Answer.Equals(answer.Answer, StringComparison.OrdinalIgnoreCase);
                    }
                }


                var requestData = new TestResultDto
                {
                    TestId = testId,
                    Answers = answers.Select(a => new TestAnswerDto
                    {
                        QuestionId = a.QuestionId,
                        Answer = a.Answer,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                };


                var response = await client.PostAsJsonAsync("api/tests/results", requestData);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Ошибка сохранения: {error}");
                    return RedirectToAction("TestError", new { testId });
                }

                var result = await response.Content.ReadFromJsonAsync<TestResultResponseDto>();
                return RedirectToAction("TestResult", new
                {
                    testId = result.TestId,
                    score = result.Score,
                    totalQuestions = result.TotalQuestions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке теста");
                return RedirectToAction("TestError", new { testId });
            }
        }
        [Authorize]
        [HttpGet("Result/{testId}/{score}/{totalQuestions}")]
        public async Task<IActionResult> TestResult(int testId, int score, int totalQuestions)
        {
            try
            {
                var userId = User.FindFirstValue("UserId");
                if (string.IsNullOrEmpty(userId))
                    return RedirectToAction("Login", "Account");

                var client = _httpClientFactory.CreateClient("ApiClient");

                // Получаем токен из куки
                var token = Request.Cookies["access_token"];
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Получаем информацию о тесте
                var testResponse = await client.GetAsync($"api/tests/{testId}/info");
                if (!testResponse.IsSuccessStatusCode)
                    return View("Error", new ErrorViewModel { ErrorMessage = "Не удалось загрузить тест" });

                var testInfo = await testResponse.Content.ReadFromJsonAsync<TestInfo>();

                // Получаем вопросы теста
                var questionsResponse = await client.GetAsync($"api/tests/{testId}/questions");
                if (!questionsResponse.IsSuccessStatusCode)
                    return View("Error", new ErrorViewModel { ErrorMessage = "Не удалось загрузить вопросы" });

                var questions = await questionsResponse.Content.ReadFromJsonAsync<List<QuestionDto>>();

                // Получаем ответы пользователя
                var answersResponse = await client.GetAsync($"api/tests/{testId}/user-answers/{userId}");
                var userAnswers = answersResponse.IsSuccessStatusCode
                    ? await answersResponse.Content.ReadFromJsonAsync<List<UserAnswer>>()
                    : new List<UserAnswer>();

                var model = new TestResultViewModel
                {
                    TestId = testId,
                    TestName = testInfo?.Name ?? "Тест",
                    Score = score,
                    TotalQuestions = totalQuestions,
                    Questions = questions.Select(q => new QuestionResultViewModel
                    {
                        QuestionId = q.Id,
                        QuestionText = q.QuestionText,
                        CorrectAnswer = q.CorrectAnswer,
                        UserAnswer = userAnswers.FirstOrDefault(a => a.QuestionId == q.Id)?.Answer,
                        IsCorrect = userAnswers.Any(a => a.QuestionId == q.Id && a.IsCorrect),
                        Explanation = $"Правильный ответ: {q.CorrectAnswer}"
                    }).ToList(),
                    QuestionResults = questions.Select(q => new QuestionResultViewModel
                    {
                        QuestionId = q.Id,
                        QuestionText = q.QuestionText,
                        CorrectAnswer = q.CorrectAnswer,
                        UserAnswer = userAnswers.FirstOrDefault(a => a.QuestionId == q.Id)?.Answer,
                        IsCorrect = userAnswers.Any(a => a.QuestionId == q.Id && a.IsCorrect),
                        Explanation = $"Правильный ответ: {q.CorrectAnswer}"
                    }).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки результатов теста");
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }

        [HttpGet("Error/{testId}")]
        public IActionResult TestError(int testId)
        {
            var errorMessage = TempData["ErrorMessage"]?.ToString()
                ?? "Произошла ошибка при обработке теста";

            return View("Error", new ErrorViewModel
            {
                ErrorMessage = $"{errorMessage}. Тест ID: {testId}",
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ReturnUrl = Url.Action("Start", new { id = testId })
            });
        }
       
}
    
}
