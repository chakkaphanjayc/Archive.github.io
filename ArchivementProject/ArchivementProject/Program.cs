using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ArchivementProject;
using ArchivementProject.Script;
using Microsoft.AspNetCore.Components.Authorization;
using PlayFab;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

PlayFabSettings.staticSettings.TitleId = "8CBAD";
PlayFabSettings.staticSettings.DeveloperSecretKey = "RZJ5K4AP5GTAKQ5IOO1TFYFUF6KFX1YHMRMMF7CJQF6ESA4FAR";

builder.Services.AddScoped<AuthenticationStateProvider, PlayFabAuthenticationStateProvider>();

builder.Services.AddScoped<UserDataService>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddOidcAuthentication(options =>
{
    // Configure your authentication provider options here.
    // For more information, see https://aka.ms/blazor-standalone-auth
    builder.Configuration.Bind("PlayFabAuthentication", options.ProviderOptions);
});

await builder.Build().RunAsync();