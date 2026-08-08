using Sneaker_Store.Model;

namespace Sneaker_Store.Services
{
    public interface IKundeRepository
    {
        void AddUser(Kunde kunde, string plainTextPassword);
        Kunde? FindByEmail(string email);
        Kunde? FindById(int kundeId);
        bool VerifyPassword(Kunde kunde, string plainTextPassword);
        void RemoveUser(Kunde kunde);
    }
}
