using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;

namespace Veracity.Common.OAuth.Providers
{
    public static class VeracityIdExtensions
    {
        public static AuthenticationBuilder AddVeracityAuthentication(this AuthenticationBuilder builder, Action<AzureAdB2COptions> options)
        {

            builder.AddAzureAdB2C(options);
            return builder;
        }
        public static AuthenticationBuilder AddVeracityAuthentication(this AuthenticationBuilder builder, IConfiguration configuration)
        {
            builder.AddVeracityAuthentication(options =>
            {
                configuration.Bind("AzureAdB2C", options);

            });
            return builder;
        }

        public static IApplicationBuilder UseVeracity(this IApplicationBuilder app)
        {

            //OauthAttribute.SetOauthProvider(new TokenProvider(app.ApplicationServices));
            return app;
        }
    }
}