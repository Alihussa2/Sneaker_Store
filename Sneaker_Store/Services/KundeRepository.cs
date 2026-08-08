using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sneaker_Store.Data;
using Sneaker_Store.Model;

namespace Sneaker_Store.Services;

public class KundeRepository : IKundeRepository
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<Kunde> _hasher = new();

    public KundeRepository(AppDbContext db)
    {
        _db = db;
    }

    public void AddUser(Kunde kunde, string plainTextPassword)
    {
        kunde.Kode = _hasher.HashPassword(kunde, plainTextPassword);
        _db.Kunder.Add(kunde);
        _db.SaveChanges();
    }

    public void RemoveUser(Kunde kunde)
    {
        _db.Kunder.Remove(kunde);
        _db.SaveChanges();
    }

    public Kunde? FindByEmail(string email)
    {
        return _db.Kunder.FirstOrDefault(k => k.Email == email);
    }

    public Kunde? FindById(int kundeId)
    {
        return _db.Kunder.Find(kundeId);
    }

    public bool VerifyPassword(Kunde kunde, string plainTextPassword)
    {
        var result = _hasher.VerifyHashedPassword(kunde, kunde.Kode, plainTextPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
