using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Veracity.Common.Authentication
{
    public class AzureAdB2COptions:TokenProviderConfiguration
    {
        public const string PolicyAuthenticationProperty = "Policy";

        public string Instance { get; set; }

        public string Domain { get; set; }

        public string EditProfilePolicyId { get; set; }

        public string SignUpSignInPolicyId { get; set; }

        public string ResetPasswordPolicyId { get; set; }

        public string CallbackPath { get; set; }

        public string DefaultPolicy => SignUpSignInPolicyId;

        public static bool TerminateOnPolicyException { get; set; }

        public OpenIdConnectEvents OpenIdConnectEvents { get; set; } = new();
    }
}