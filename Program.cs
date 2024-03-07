using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SocialMediaWisLam.Data;
using SocialMediaWisLam.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using SocialMediaWisLam.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SocialMediaWisLamContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SocialMediaWisLamContext") ?? throw new InvalidOperationException("Connection string 'SocialMediaWisLamContext' not found.")));

builder.Services.AddDefaultIdentity<Profile>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<SocialMediaWisLamContext>();
/*builder.Services.AddIdentityCore<Profile>(opt => { opt.User.RequireUniqueEmail = true; })
    .AddRoles<IdentityRole>()
    .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory<Profile, IdentityRole>>()
    .AddSignInManager<SignInManager<Profile>>().AddEntityFrameworkStores<SocialMediaWisLamContext>()
    .AddDefaultTokenProviders();*/

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    SeedData.Initialize(services);
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
