using Microsoft.EntityFrameworkCore;
using Sneaker_Store.Data;
using Sneaker_Store.Model;

namespace Sneaker_Store.Services
{
    public class SkoRepository : ISkoRepository
    {
        private readonly AppDbContext _db;

        public SkoRepository(AppDbContext db)
        {
            _db = db;
        }

        public List<Sko> GetAll()
        {
            return _db.Sko.AsNoTracking().ToList();
        }

        public Sko GetById(int skoid)
        {
            Sko? sko = _db.Sko.Find(skoid);
            if (sko is null)
            {
                throw new KeyNotFoundException();
            }
            return sko;
        }

        public Sko Add(Sko sko)
        {
            _db.Sko.Add(sko);
            _db.SaveChanges();
            return sko;
        }

        public Sko Delete(int skoid)
        {
            Sko deleteSko = GetById(skoid);
            _db.Sko.Remove(deleteSko);
            _db.SaveChanges();
            return deleteSko;
        }

        public Sko Update(int skoid, Sko updatedSko)
        {
            if (skoid != updatedSko.SkoId)
            {
                throw new ArgumentException("kan ikke opdatere id og obj.Id er forskellige");
            }

            Sko updateThisSko = GetById(skoid);

            updateThisSko.Maerke = updatedSko.Maerke;
            updateThisSko.Model = updatedSko.Model;
            updateThisSko.Str = updatedSko.Str;
            updateThisSko.Pris = updatedSko.Pris;
            updateThisSko.LagerAntal = updatedSko.LagerAntal;
            updateThisSko.Billede = updatedSko.Billede;

            _db.SaveChanges();
            return updateThisSko;
        }

        public Sko ReducerLager(int skoid, int antal)
        {
            Sko sko = GetById(skoid);
            if (antal > sko.LagerAntal)
            {
                throw new InvalidOperationException($"Kun {sko.LagerAntal} stk. på lager.");
            }

            sko.LagerAntal -= antal;
            _db.SaveChanges();
            return sko;
        }
    }
}
