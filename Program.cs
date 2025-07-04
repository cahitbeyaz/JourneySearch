using ObiletJourneySearch.Middleware;
using ObiletJourneySearch.Services.Caching;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure HTTP context accessor for getting client IP address
builder.Services.AddHttpContextAccessor();

// Add memory cache
builder.Services.AddMemoryCache();

// Register cache services
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.AddSingleton<ILocationCacheService, LocationCacheService>();

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register Obilet API client
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ObiletJourneySearch.ApiClient.IObiletApiClient, ObiletJourneySearch.ApiClient.ObiletApiClient>();
builder.Services.AddScoped<ObiletJourneySearch.Services.ISessionService, ObiletJourneySearch.Services.SessionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
else
{
    // In development, we'll see detailed error information
    app.UseDeveloperExceptionPage();
}

// Register global exception handler middleware
app.UseMiddleware<ObiletJourneySearch.Middleware.ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Enable session
app.UseSession();

// Register the Obilet session middleware
app.UseObiletSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
