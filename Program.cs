using AIMoneyRecordLineBot.Entity;
using AIMoneyRecordLineBot.Models;
using AIMoneyRecordLineBot.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services.AddOpenApi();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<ExpenseRecordService>();

var configuration = builder.Configuration;
var channelAccessToken = configuration["LineBotChannelAccessToken"] ?? "";
var OpenaiApiKey = configuration["OpenaiApiKey"] ?? "";
var GeminiApiKey = configuration["GeminiApiKey"] ?? "";
var LiffId = configuration["LiffId"] ?? "";
builder.Services.AddDbContext<AIMoneyRecordLineBotContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.Configure<LineBotSettings>(settings =>
{
    settings.ChannelAccessToken = channelAccessToken;
    settings.OpenaiApiKey = OpenaiApiKey;
    settings.GeminiApiKey = GeminiApiKey;
    settings.LiffId = LiffId;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthorization();
app.UseStaticFiles();
app.UseRouting();
//app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
