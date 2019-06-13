namespace Veracity.Common.Authentication
{
    public interface IDataProtector
    {
        byte[] Protect(byte[] data);
        byte[] Unprotect(byte[] data);
    }
}