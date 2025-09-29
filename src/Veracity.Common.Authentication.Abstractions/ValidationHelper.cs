using System;

namespace Veracity.Common.Authentication
{
    public static class ValidationHelper
    {
        public static void ValidateRequiredString(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(paramName, $"{paramName} cannot be null or empty");
        }
    }
}