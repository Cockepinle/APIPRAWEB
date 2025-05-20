using PRAAPIWEB.Services;

var builder = WebApplication.CreateBuilder(args);

// ������ ������� URL �� ��������
var apiBaseUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl");

// ������������ HttpClient ��� AboutProjectSectionService
builder.Services.AddHttpClient<AboutProjectSectionService>(client =>
{
    var baseUri = apiBaseUrl.EndsWith("/") ? apiBaseUrl : apiBaseUrl + "/";
    client.BaseAddress = new Uri(baseUri);
});

// ������������ MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=AboutProject}/{action=Index}/{id?}");

app.Run();
