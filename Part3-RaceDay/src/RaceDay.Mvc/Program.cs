using RaceDay.Mvc.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC + session state (holds the API token and the signed-in user's id and role).
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".RaceDay.Session";
});

// Typed client used for every call to the RaceDay API. All data operations in
// this application go through ApiClient - the MVC project has no database access.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5132/";
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();      // must run before controllers that read session state
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
