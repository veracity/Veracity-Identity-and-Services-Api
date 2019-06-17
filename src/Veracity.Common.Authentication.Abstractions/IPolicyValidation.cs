using System.Threading.Tasks;

namespace Veracity.Common.Authentication
{
    public interface IPolicyValidation
    {
        Task<ValidationResult> ValidatePolicy(string protocolMessageRedirectUri);
        Task<ValidationResult> ValidatePolicyWithServiceSpesificTerms( string serviceId, string protocolMessageRedirectUri);
    }
}