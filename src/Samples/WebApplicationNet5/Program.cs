using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace WebApplicationNet7
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                var builtConfig = config.Build();

                var keyVaultEndpoint = builtConfig["KeyVaultEndpoint"];
                if (!string.IsNullOrEmpty(keyVaultEndpoint))
                {
                    var secretClient = new SecretClient(
                        new Uri(keyVaultEndpoint),
                        new DefaultAzureCredential());
                    config.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
                    config.AddEnvironmentVariables();
                }
            })     

            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
