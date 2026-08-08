using Sneaker_Store.Model;

namespace Sneaker_Store.Services
{
    public interface ISkoRepository
    {
        Sko Add(Sko newSko);
        List<Sko> GetAll();
        Sko GetById(int skoid);
        Sko Delete(int skoid);
        Sko Update(int skoid, Sko updatedSko);
        Sko ReducerLager(int skoid, int antal);
    }
}