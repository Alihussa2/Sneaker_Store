using Sneaker_Store.Data;
using Sneaker_Store.Model;

namespace Sneaker_Store.Services
{
    public class KvitteringRepository : IKvitteringRepository
    {
        private readonly AppDbContext _db;

        public KvitteringRepository(AppDbContext db)
        {
            _db = db;
        }

        public void OpretKvittering(Kvittering kvittering)
        {
            _db.Kvitteringer.Add(kvittering);
            _db.SaveChanges();
        }

        public Kvittering? HentKvittering(int id)
        {
            return _db.Kvitteringer.Find(id);
        }

        public IEnumerable<Kvittering> HentAlleKvitteringer()
        {
            return _db.Kvitteringer.ToList();
        }
    }
}
