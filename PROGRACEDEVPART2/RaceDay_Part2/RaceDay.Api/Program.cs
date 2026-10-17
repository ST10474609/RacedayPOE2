using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "RaceDay API", Version = "v1", Description = "RESTful API for RaceDay event management." });
    var xmlPath = Path.Combine(AppContext.BaseDirectory, "RaceDay.Api.xml");
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
});
builder.Services.AddDbContext<RaceDayDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("RaceDay")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(60); options.Cookie.HttpOnly = true; options.Cookie.IsEssential = true; });
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => {
    options.Cookie.Name = "RaceDay.Auth"; options.Cookie.HttpOnly = true; options.Cookie.SameSite = SameSiteMode.Lax; options.ExpireTimeSpan = TimeSpan.FromMinutes(60); options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
    options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
});
builder.Services.AddScoped<IPasswordService, PasswordService>();
var app = builder.Build();
using (var scope = app.Services.CreateScope()) { if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing")) scope.ServiceProvider.GetRequiredService<RaceDayDbContext>().Database.EnsureCreated(); }
app.UseSwagger(); app.UseSwaggerUI(); if (!app.Environment.IsEnvironment("Testing")) app.UseHttpsRedirection(); app.UseSession(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers(); app.Run();
public partial class Program { }
