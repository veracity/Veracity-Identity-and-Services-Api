using Microsoft.AspNetCore.DataProtection;

namespace Veracity.Common.Authentication
{
    public class DataProtectorNetCore : DataProtector
    {
        private Microsoft.AspNetCore.DataProtection.IDataProtector _protector;

        public DataProtectorNetCore(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("tokenProtector");
        }
        public override byte[] Protect(byte[] data)
        {
            if (data == null) return null;
            return _protector.Protect(data);
        }

        public override byte[] Unprotect(byte[] data)
        {
            if (data == null) return null;
            return _protector.Unprotect(data);
        }
    }
}
