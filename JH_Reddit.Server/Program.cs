using Newtonsoft.Json.Linq;

var builder = WebApplication.CreateBuilder(args);

var appSettings = builder.Configuration.GetSection("AppSettings");
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient("redditOauthClient", httpClient =>
{
    httpClient.BaseAddress = new Uri(appSettings["oauth-base-url"] ?? "https://oauth.reddit.com");
    httpClient.DefaultRequestHeaders.Add("User-Agent", appSettings["user-agent"]);
    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {appSettings["token"]}");
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
