using Sneaker_Store.Model;

namespace Sneaker_Store.Services

{
    public interface IKvitteringRepository
    {
        // Metoder til at arbejde med kvitteringer

        // Opret en ny kvittering
        void OpretKvittering(Kvittering kvittering);

        // Hent en kvittering baseret på ID
        Kvittering? HentKvittering(int id);

        // Hent alle kvitteringer
        IEnumerable<Kvittering> HentAlleKvitteringer();

        
        
    }
}