using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using DigitalSeal.Web.Models.ConfiurationModels;
using DigitalSeal.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.StaticFiles;
using DigitalSeal.Core.ListProviders.DocPartyList;
using DigitalSeal.Core.ListProviders.DocPartyPossibList;
using DigitalSeal.Core.ListProviders.OrgList;
using DigitalSeal.Core.ListProviders.DocList;
using DigitalSeal.Core.ListProviders.PartyList;
using DigitalSeal.Core.ListProviders.PartyPendingList;
using DigitalSeal.Core.ListProviders.PartyPossibList;
using DigitalSeal.Core.Services;
using DigitalSeal.Core.Models.Config.Email;
using DigitalSeal.Core.Models.Config;
using DigitalSeal.Core.ListProviders.Signatures;
using DigitalSeal.Data.Models;
using DigitalSeal.Data;
using DigitalSeal.Data.Services;
using DigitalSeal.Core;
using DigitalSeal.Web.Filters;
using System.Reflection;
using DigitalSeal.Core.Utilities;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IServiceCollection services = builder.Services;
ConfigurationManager configuration = builder.Configuration;

//services.AddDbContext<AppDbContext>(options => options
//    .UseSqlServer(builder.Configuration.GetConnectionString("Local")));

services.AddDataServices(configuration);

services.AddCoreServices(configuration);

services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

services.AddAuthentication()
    .AddCookie(options =>
    {
        options.Cookie.IsEssential = true;
        //options.Cookie.SameSite = SameSiteMode.None;
    })
    .AddGoogle(options =>
    {
        OAuthConfig oAuthConfig = configuration
            .GetSection("Authentication")
            .GetSection("Google")
            .Get<OAuthConfig>()!;

        options.ClientId = oAuthConfig.ClientId;
        options.ClientSecret = oAuthConfig.ClientSecret;
    });

services.Configure<IdentityOptions>(options => 
{
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
});

services.AddSession();

services.ConfigureApplicationCookie(options => options.LoginPath = "/account/login");

services.AddControllersWithViews(options => options.Filters.Add<ValidationFilter>())
    .AddRazorRuntimeCompilation()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(SharedResources));
        //opts.DataAnnotationLocalizerProvider = (type, factory) =>
        //{
        //    var assemblyName = new AssemblyName(typeof(SharedResource).GetTypeInfo().Assembly.FullName!);
        //    return factory.Create(nameof(SharedResource), assemblyName.Name!);
        //};
    });

services.AddLocalization(options => options.ResourcesPath = "Resources");
services.AddMvc(options => options.Filters.Add(new ResponseCacheAttribute
{
    NoStore = true,
    Location = ResponseCacheLocation.None
})).AddViewLocalization();

_ = services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

services.AddScoped<IDataSeeder, DataSeeder>();
services.AddScoped<IAccountService, AccountService>();
services.AddScoped<IMessageCreator, MessageCreator>();

services.Configure<LoginOptions>(configuration.GetSection("Login"));
services.Configure<DocumentOptions>(configuration.GetSection("Document"));
services.Configure<AesOptions>(configuration.GetSection("Aes"));
services.Configure<EmailOptions>(configuration.GetSection("EmailConfiguration"));

services.AddNotyf(config => 
{
    //config.DurationInSeconds = 5; 
    config.DurationInSeconds = 99999;
    config.HasRippleEffect = true;
    config.IsDismissable = true;
    config.Position = NotyfPosition.TopRight;
});
services.AddScoped<AppSignInManager>();
services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(10));

var app = builder.Build();

using (var serviceScope = app.Services.CreateScope())
{
    await DataSeeder.EnsureDbCreated(serviceScope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/DocList/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

var supportedCultures = new[] { new CultureInfo("en-US") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});
app.UseNotyf();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".ftl"] = "application/octet-stream";
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

//app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=DocList}/{action=Index}");

app.Run();