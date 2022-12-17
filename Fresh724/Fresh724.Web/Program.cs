using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Fresh724.Data;
using Fresh724.Data.Context;
using Fresh724.Data.Repository.Abstract;
using Fresh724.Data.Repository.Concrete;
using Fresh724.Entity.Entities;
using Stripe;
using Fresh724.Service;
using Microsoft.AspNetCore.Identity.UI.Services;
using ApplicationDbInitializer = Fresh724.Data.ApplicationDbInitializer;

var builder = WebApplication.CreateBuilder(args);

// her i make configuration for variable for the External Login for the Facebook an google. 

var config = builder.Configuration; 

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

/*builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();*/

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(); 
builder.Services.AddSingleton<IEmailSender, EmailService>();
builder.Services.Configure<StripeService>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    //calling her the Config variable 
    googleOptions.ClientId = config["Authentication:Google:ClientId"];
    googleOptions.ClientSecret = config["Authentication:Google:ClientSecret"];
});

//.AddFacebook means calling for the Facebook Packeg
builder.Services.AddAuthentication().AddFacebook(facebookOptions =>
{
    
    //calling her the Config variable 
    facebookOptions.AppId = config["Authentication:Facebook:AppId"];
    facebookOptions.AppSecret = config["Authentication:Facebook:AppSecret"];
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var services =app.Services.CreateScope())
{
    var db = services.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var um  = services.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var rm  =  services.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    ApplicationDbInitializer.Initialize(db,um,rm);
}






// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
//SeedDatabase();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapRazorPages();

/*app.MapAreaControllerRoute(
    name: "Admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Category}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "Company",
    areaName: "Company",
    pattern: "Company/{controller=Employee}/{action=Index}/{id?}");*/

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name : "areas",
        pattern : "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
/*void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IApplicationDbInitializer>();
        dbInitializer.Initialize();
    }
}*/


/*
 {
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=Fresh724;Trusted_connection=True;"
  },
 */