using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.Extensions.Options;
using PRAAPIWEB.Services;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация API
var apiBaseUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/');
if (string.IsNullOrEmpty(apiBaseUrl))
{
    throw new InvalidOperationException("API base URL is not configured");
}

// Регистрация сервисов
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl + "/");
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
    client.Timeout = TimeSpan.FromSeconds(60); // Увеличиваем таймаут
});

// Универсальная регистрация сервисов
builder.Services.AddHttpClient<AuthService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl + "/");
    client.Timeout = TimeSpan.FromSeconds(60);
});

builder.Services.AddHttpClient<AboutProjectSectionService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl + "/");
    client.Timeout = TimeSpan.FromSeconds(60);
});

// Настройка аутентификации
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = "YourApp.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Events = new CookieAuthenticationEvents
    {
        OnValidatePrincipal = context =>
        {
            // Дополнительная валидация при каждом запросе
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.HttpOnly = HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.Always;
});
var app = builder.Build();

// Конфигурация middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();

    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCookiePolicy();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "testResult",
    pattern: "Tests/Result/{testId}",
    defaults: new { controller = "Tests", action = "TestResult" });

app.MapControllerRoute(
    name: "testError",
    pattern: "Tests/Error/{testId}",
    defaults: new { controller = "Tests", action = "TestError" });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=AboutProject}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "testError",
    pattern: "Tests/Error/{testId}",
    defaults: new { controller = "Tests", action = "TestError" });
app.Run();