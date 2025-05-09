using EtherApp.Data;
using EtherApp.Data.Helpers;
using EtherApp.Data.Hubs;
using EtherApp.Data.Models;
using EtherApp.Data.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//Database Configuration
var dbConnectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDBContext>(options => options.UseSqlServer(dbConnectionString));
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB upload
});

//Services Configuration
builder.Services.AddScoped<IPostsService, PostService>();
builder.Services.AddScoped<IHashtagsService, HashtagService>();
builder.Services.AddScoped<IStoriesService, StoriesService>();
builder.Services.AddScoped<IFilesService, FilesService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFriendsService, FriendsService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IMLInterestService, MLInterestService>();

builder.Services.AddSingleton<MLContext>(new MLContext(seed: 0));


// Identity Configuration
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    // Password Settings
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 4;
})
                .AddEntityFrameworkStores<AppDBContext>()
                .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Authentication/Login";
    options.AccessDeniedPath = "/Home/AccessDenied";
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddGoogle(o =>
    {
        o.ClientId = builder.Configuration["Auth:Google:ClientId"] ?? "";
        o.ClientSecret = builder.Configuration["Auth:Google:ClientSecret"] ?? "";
        o.CallbackPath = "/signin-google";
    });
builder.Services.AddAuthorization();

builder.Services.AddSignalR();


var app = builder.Build();

// seed the database with initial data
using (var scope = app.Services.CreateScope())
{
    //var dbConext = scope.ServiceProvider.GetRequiredService<AppDBContext>();
    //await dbConext.Database.MigrateAsync();
    //await DBInitializer.SeedAsync(dbConext);

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    await DBInitializer.SeedUsersAndRolesAsync(userManager, roleManager);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<NotificationHub>("/notificationHub");

app.Run();
