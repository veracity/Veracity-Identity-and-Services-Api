namespace Veracity.Services.Api.Extensions
{
    public abstract class ClientSuportCodeHandler
    {
        protected internal abstract void SetSupportCode(string supportCode);

        protected internal abstract string GetSupportCode();
    }
}