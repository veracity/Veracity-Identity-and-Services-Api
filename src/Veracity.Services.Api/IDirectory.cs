namespace Veracity.Services.Api
{
    public interface IDirectory
    {
        ICompaniesDirectory Companies { get; }
        IServicesDirectory Services { get; }
        IUsersDirectory Users { get; }

        IDataContainerService DataContainer { get; }
    }
}