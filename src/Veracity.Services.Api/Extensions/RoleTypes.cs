namespace Veracity.Services.Api.Extensions
{
    public sealed class RoleTypes
    {
        public const string ManageServices = "MYDNV_ADM_SERVICE";
        public const string ManageServiceSubscriptions = "MYDNV_ADM_SUBS";
        public const string ManageServiceAdministrators = "MYDNV_ADM_ADMUSR";
        public const string IsValidUser = "ValidUser";
        public const string ReadAccess = "MYDNV_ADM_READ";
        public const string AsService = "MYDNV_ADM_READ";//TODO: change to MYDNV_ADM_SERVICE_ACCESS ???
        public const string ReadDirectoryAccess = "MYDNV_ADM_READDIR"; //TODO: Change to read directory when created
    }
}