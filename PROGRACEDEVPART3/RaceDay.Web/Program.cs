using RaceDay.Web.Services;
var builder=WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o=>{o.IdleTimeout=TimeSpan.FromMinutes(60);o.Cookie.HttpOnly=true;o.Cookie.IsEssential=true;o.Cookie.Name=".RaceDay.Web.Session";o.Cookie.SameSite=SameSiteMode.Lax;});
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("RaceDayApi",client=>{var url=builder.Configuration["Api:BaseUrl"]??"https://localhost:7200/";client.BaseAddress=new Uri(url.EndsWith('/')?url:url+"/");client.Timeout=TimeSpan.FromSeconds(30);});
builder.Services.AddScoped<RaceDayApiClient>();
var app=builder.Build();
if(!app.Environment.IsDevelopment()){app.UseExceptionHandler("/Home/Error");app.UseHsts();}
app.UseHttpsRedirection();app.UseStaticFiles();app.UseRouting();app.UseSession();
app.MapControllerRoute(name:"default",pattern:"{controller=Events}/{action=Index}/{id?}");
app.Run();