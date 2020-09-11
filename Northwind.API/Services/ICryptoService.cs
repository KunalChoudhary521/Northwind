namespace Northwind.API.Services
{
    public interface ICryptoService
    {
        bool IsPasswordCorrect(string password, byte[] testSalt, byte[] testHash);
        void EncryptPassword(string password, out byte[] salt, out byte[] hash);
    }
}