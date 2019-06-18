namespace Veracity.Services.Api
{
    public interface IApiClient
    {
        IMy My { get; }
        IThis This { get; }
        IDirectory Directory { get; }
    }
}