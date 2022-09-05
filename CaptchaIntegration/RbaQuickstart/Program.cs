using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rsk.RiskBasedAuthentication.Configuration.DependancyInjection;
using Rsk.RiskBasedAuthentication.Detectors;
using Rsk.RiskBasedAuthentication.RbaOptions;
using Rsk.RiskBasedAuthentication.Storage.EntityFramework.Configuration;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddRiskBasedAuthentication(options =>
    {
        options.Licensee = "";
        options.LicenseKey = "";
    })
    .AddAlertServiceInMemoryCache()
    .AddFailedAuthenticationSpikeDetector(options => {

        options = new FailedAuthenticationDetectorOptions.HighSensitivity();
        options.SeedProfile = SeedProfiles.DefaultProfile();
    })
    .AddFailedAuthenticationSpikeDetectorStore(optionsBuilder => optionsBuilder.UseSqlServer("", b => b.MigrationsAssembly("RbaQuickstart")));

builder.Services.AddDbContext<IdentityDbContext>(optionsBuilder => optionsBuilder.UseSqlServer("", b => b.MigrationsAssembly("RbaQuickstart")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<IdentityDbContext>();

var app = builder.Build();

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

app.Run();
