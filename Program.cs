using ObiletJourneySearch.ApiClient;
using ObiletJourneySearch.Middleware;
using ObiletJourneySearch.Services.Caching;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.AddSingleton<ILocationCacheService, LocationCacheService>();
builder.Services.AddHttpClient<IObiletApiClient, ObiletApiClient>((serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var token = config["ObiletApi:ApiClientToken"];

    client.BaseAddress = new Uri("https://v2-api.obilet.com/api/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
});
builder.Services.AddScoped<ObiletJourneySearch.Services.ISessionService, ObiletJourneySearch.Services.SessionService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseMiddleware<ObiletJourneySearch.Middleware.ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseObiletSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
