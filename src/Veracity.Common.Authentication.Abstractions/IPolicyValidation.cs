using System.Threading.Tasks;

namespace Veracity.Common.Authentication
{
    public interface IPolicyValidation
    {
        Task<ValidationResult> ValidatePolicy();
        Task<ValidationResult> ValidatePolicyWithServiceSpesificTerms(string serviceId);
    }
}