using Sneaker_Store.Data;
using Sneaker_Store.Model;

namespace Sneaker_Store.Services
{
    public class OrdreRepository : IOrdreRepository
    {
        private readonly AppDbContext _db;

        public OrdreRepository(AppDbContext db)
        {
            _db = db;
        }

        public void TilføjOrdre(Ordre ordre)
        {
            _db.Ordrer.Add(ordre);
            _db.SaveChanges();
        }

        public Ordre? FindOrdre(int ordreId)
        {
            return _db.Ordrer.Find(ordreId);
        }

        public IEnumerable<Ordre> HentAlleOrdrer()
        {
            return _db.Ordrer.ToList();
        }

        public void OpdaterOrdre(Ordre ordre)
        {
            var existing = _db.Ordrer.Find(ordre.OrdreId);
            if (existing is null)
            {
                throw new KeyNotFoundException();
            }

            existing.KundeId = ordre.KundeId;
            existing.SkoId = ordre.SkoId;
            existing.Antal = ordre.Antal;
            existing.TotalPris = ordre.TotalPris;
            _db.SaveChanges();
        }

        public void SletOrdre(int ordreId)
        {
            var existing = _db.Ordrer.Find(ordreId);
            if (existing is not null)
            {
                _db.Ordrer.Remove(existing);
                _db.SaveChanges();
            }
        }
    }
}
