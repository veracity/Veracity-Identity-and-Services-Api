// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Stardust.Interstellar.Rest.Client;
using Stardust.Particles;
using System.Diagnostics;
using Veracity.Common.Authentication;
using Veracity.Common.OAuth.Providers;
using Veracity.Services.Api;

Console.WriteLine("Welcome to the Client Credentials sample");
IServiceCollection services = new ServiceCollection();
var builder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true, true)
    //.AddAzureKeyVault(new Uri("https://veracitydevdaydemo.vault.azure.net/"), new DefaultAzureCredential())
    .AddEnvironmentVariables();
var config = builder.Build();
services.AddVeracityDeamonApp(config).AddScoped<IOptions>(c=>c.CreateRestClient<Veracity.Services.Api.IOptions>(ConfigurationManagerHelper.GetValueOnKey("myApiV3Url")))
    .AddVeracityServices(ConfigurationManagerHelper.GetValueOnKey("myApiV3Url"));
var provider = services.BuildServiceProvider();
var user = await provider.GetService<IThis>().ResolveUser("jonas.syrstad@dnv.com");// warm-up call.
var timer = Stopwatch.StartNew();
user = await provider.GetService<IThis>().ResolveUser("jsyrstad2@gmail.com");
timer.Stop();
Console.WriteLine(JsonConvert.SerializeObject(user, Formatting.Indented));
Console.WriteLine($"Execution time: {timer.ElapsedMilliseconds}ms");

