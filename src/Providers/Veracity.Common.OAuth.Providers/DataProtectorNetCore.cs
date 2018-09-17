using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace Veracity.Common.OAuth.Providers
{
    public class DataProtectorNetCore : DataProtector
    {
        private readonly IDataProtectionProvider _provider;
        private Microsoft.AspNetCore.DataProtection.IDataProtector _protector;

        public DataProtectorNetCore(IDataProtectionProvider provider)
        {
            _provider = provider;
            _protector = _provider.CreateProtector("tokenProtector");
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
