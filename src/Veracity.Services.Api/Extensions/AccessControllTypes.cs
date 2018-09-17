namespace Veracity.Services.Api.Extensions
{
    public enum AccessControllTypes
    {
        User,
        Service,
        /// <summary>
        /// Allows access if the user has the required role and the clientId has read access (access to service)
        /// Allows acces if the clientId has both the required role and is allowed to act alone
        /// </summary>
        UserAndService,
        /// <summary>
        /// if the request contains a service account the access rights from this is used first, if it doesnt have the rights then the  user principal will be used.
        /// </summary>
        ServiceThenUser,

        /// <summary>
        /// Uses the users rights if present, else the services
        /// </summary>
        UserOrService

    }
}