namespace Sneaker_Store.Model;

public class Kvittering
{
    public int Id { get; set; }
    public int KundeId { get; set; }
    public int Antal { get; set; }
    public double TotalPris { get; set; }
    public string Beskrivelse { get; set; }
    public DateTime Koebsdato { get; set; }

    public Kvittering()
    {
        Beskrivelse = "";
        Koebsdato = DateTime.Now;
    }

    public Kvittering(int id, int kundeId, int antal, double totalPris, string beskrivelse, DateTime koebsdato)
    {
        Id = id;
        KundeId = kundeId;
        Antal = antal;
        TotalPris = totalPris;
        Beskrivelse = beskrivelse;
        Koebsdato = koebsdato;
    }

    public override string ToString()
    {
        return $"{nameof(Id)}: {Id}, {nameof(KundeId)}: {KundeId}, {nameof(Antal)}: {Antal}, {nameof(TotalPris)}: {TotalPris}, {nameof(Beskrivelse)}: {Beskrivelse}, {nameof(Koebsdato)}: {Koebsdato}";
    }
}
