using DataPersist.SavedViews;
using DataPersist.SavedViews.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Caching 

builder.Services.AddScoped<ICache, Cache>();
builder.Services.AddScoped<ILookup, Lookup>();
builder.Services.AddScoped<IFilter, Filter>();
builder.Services.AddScoped<IRollup, Rollup>();
builder.Services.AddScoped<ISearch, Search>();

// Identity support

builder.Services.AddSingleton<ICurrentUser, CurrentUser>();

// Applications services

builder.Services.AddScoped<IViewedService, ViewedService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IHistoryService, HistoryService>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = new PathString("/login");
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
});

builder.Services.AddHttpContextAccessor();

// Database contexts: For localdb connectionString's path is calculated

var connectionString = builder.Configuration.GetConnectionString("University")
                              .Replace("{Path}", builder.Environment.ContentRootPath);

builder.Services.AddDbContext<UniversityContext>(options =>
                         options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.Filters.Add(typeof(GlobalExceptionFilter));
    options.Filters.Add(typeof(ControllerAccessorFilter)); // required for ultra-clean architecture
})
   .AddFlatAreas(new FlatAreaOptions())
   .AddRazorRuntimeCompilation();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// ----

var app = builder.Build();

// Setup service locator

var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
ServiceLocator.Register(httpContextAccessor);

// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
